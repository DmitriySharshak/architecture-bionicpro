# dags/dag_bionicpro.py - версия без плагина
from datetime import datetime, timedelta
from airflow import DAG
from airflow.providers.postgres.hooks.postgres import PostgresHook
from airflow.operators.python import PythonOperator
from airflow.operators.dummy import DummyOperator
import logging
import requests
import json
import socket

logger = logging.getLogger(__name__)

default_args = {
    'owner': 'bionicpro',
    'depends_on_past': False,
    'start_date': datetime(2024, 1, 1),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 1,
    'retry_delay': timedelta(minutes=2),
}

dag = DAG(
    'bionicpro_report_generation',
    default_args=default_args,
    description='ETL для витрины отчётов BionicPRO',
    schedule_interval='0 2 * * *',
    catchup=False,
    tags=['bionicpro', 'reports', 'etl'],
    max_active_runs=1
)

class ClickHouseHTTPHook:
    """Класс для работы с ClickHouse через HTTP API"""
    
    def __init__(self, host='olap_db', port=8123, user='default', password='', timeout=30):
        self.host = host
        self.port = port
        self.base_url = f"http://{host}:{port}"
        self.auth = (user, password) if password else None
        self.timeout = timeout
    
    def test_connection(self):
        """Тестирование подключения к ClickHouse"""
        logger.info(f"=== Тест подключения к ClickHouse ===")
        logger.info(f"Хост: {self.host}")
        logger.info(f"Порт: {self.port}")
        logger.info(f"URL: {self.base_url}")
        
        # 1. Проверка разрешения DNS
        try:
            logger.info(f"Проверка DNS резолвинга для {self.host}...")
            ip = socket.gethostbyname(self.host)
            logger.info(f"✅ DNS резолвинг успешен: {self.host} -> {ip}")
        except socket.gaierror as e:
            logger.error(f"❌ Ошибка DNS резолвинга: {e}")
            return False
        
        # 2. Проверка TCP подключения
        try:
            logger.info(f"Проверка TCP подключения к {self.host}:{self.port}...")
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(5)
            result = sock.connect_ex((self.host, self.port))
            sock.close()
            
            if result == 0:
                logger.info(f"✅ TCP подключение успешно")
            else:
                logger.error(f"❌ TCP подключение не удалось (код ошибки: {result})")
                return False
        except Exception as e:
            logger.error(f"❌ Ошибка TCP подключения: {e}")
            return False
        
        # 3. Проверка HTTP запроса к ClickHouse
        try:
            logger.info("Проверка HTTP запроса (ping)...")
            response = requests.get(
                f"{self.base_url}/ping",
                timeout=self.timeout
            )
            if response.status_code == 200:
                logger.info(f"✅ HTTP подключение успешно (статус: {response.status_code})")
                if response.text == 'Ok':
                    logger.info("✅ ClickHouse отвечает 'Ok'")
            else:
                logger.warning(f"⚠️ HTTP статус: {response.status_code}")
        except Exception as e:
            logger.error(f"❌ Ошибка HTTP запроса: {e}")
            return False
        
        # 4. Проверка версии ClickHouse
        try:
            logger.info("Проверка версии ClickHouse...")
            response = requests.get(
                f"{self.base_url}/?query=SELECT version()",
                timeout=self.timeout
            )
            if response.status_code == 200:
                logger.info(f"✅ Версия ClickHouse: {response.text.strip()}")
            else:
                logger.warning(f"⚠️ Не удалось получить версию: {response.status_code}")
        except Exception as e:
            logger.warning(f"⚠️ Ошибка получения версии: {e}")
        
        logger.info("=== Тест подключения завершен ===")
        return True
    

    def execute(self, sql: str):
        """Выполнение SQL-запроса в ClickHouse"""
        response = requests.post(
            f"{self.base_url}/",
            params={'query': sql},
            auth=self.auth,
            headers={'Content-Type': 'text/plain'}
        )
        
        if response.status_code != 200:
            raise Exception(f"ClickHouse error {response.status_code}: {response.text}")
        
        return response.text
    
    def get_records(self, sql: str):
        """Получение записей из ClickHouse в виде списка кортежей"""
        logger.info(f"Выполнение запроса к ClickHouse...")
        logger.debug(f"SQL: {sql}")
        
        # Параметр format нужно передавать как часть URL параметра query, а не отдельно
        # Просто добавляем FORMAT в сам SQL запрос
        sql_with_format = f"{sql} FORMAT JSONCompact"
        
        response = requests.post(
            f"{self.base_url}/",
            params={'query': sql_with_format},  # <-- format встроен в SQL
            auth=self.auth,
            headers={'Content-Type': 'text/plain'},
            timeout=self.timeout
        )
        
        if response.status_code != 200:
            logger.error(f"ClickHouse ошибка: {response.text[:500]}")
            raise Exception(f"ClickHouse error: {response.text}")
        
        data = response.json()
        records_count = len(data.get('data', []))
        logger.info(f"Получено {records_count} записей")
        
        return [tuple(row) for row in data.get('data', [])]
    # def get_records(self, sql: str):
    #     """Получение записей из ClickHouse в виде списка кортежей"""
    #     response = requests.post(
    #         f"{self.base_url}/",
    #         params={'query': sql, 'format': 'JSONCompact'},
    #         auth=self.auth,
    #         headers={'Content-Type': 'text/plain'}
    #     )
        
    #     if response.status_code != 200:
    #         raise Exception(f"ClickHouse error: {response.text}")
        
    #     data = response.json()
    #     return [tuple(row) for row in data.get('data', [])]

def extract_users_from_crm(**context):
    """Извлечение данных пользователей из CRM"""
    logger.info("Извлечение данных из CRM...")
    
    pg_hook = PostgresHook(postgres_conn_id='bionicpro_crm')
    
    sql = """
        SELECT id, name, email, age, gender, country
        FROM users
        ORDER BY id
    """
    
    records = pg_hook.get_records(sql)
    
    users = []
    for record in records:
        users.append({
            'user_id': record[0],
            'user_name': record[1],
            'user_email': record[2],
            'age': record[3],
            'gender': record[4],
            'country': record[5]
        })
    
    logger.info(f"Извлечено {len(users)} пользователей")
    context['task_instance'].xcom_push(key='users_data', value=users)
    return users

def extract_telemetry(**context):
    """Извлечение телеметрии из ClickHouse"""
    logger.info("Извлечение телеметрии из ClickHouse...")
    
    end_date = datetime.now()
    start_date = end_date - timedelta(days=1000)
    
    # Создаем экземпляр с таймаутом
    ch_hook = ClickHouseHTTPHook(host='olap_db', port=8123, timeout=30)
    
    # Проверяем подключение перед запросом
    if not ch_hook.test_connection():
        raise Exception("Не удалось подключиться к ClickHouse")

    sql = f"""
        SELECT 
            user_id,
            prosthesis_type,
            muscle_group,
            count(*) as signal_count,
            avg(signal_frequency) as avg_freq,
            avg(signal_duration) as avg_duration,
            avg(signal_amplitude) as avg_amplitude,
            max(signal_time) as last_signal
        FROM bionicpro_analytics.telemetry
        GROUP BY user_id, prosthesis_type, muscle_group
    """
    #WHERE signal_time >= '{start_date.strftime("%Y-%m-%d %H:%M:%S")}'
    # Проверяем есть ли данные
    count_result = ch_hook.execute("SELECT count() FROM bionicpro_analytics.telemetry")
    logger.info(f"Количество записей в telemetry: {count_result.strip()}")

    records = ch_hook.get_records(sql)
    
    telemetry_data = []
    for record in records:
        telemetry_data.append({
            'user_id': record[0],
            'prosthesis_type': record[1],
            'muscle_group': record[2],
            'signal_count': record[3],
            'avg_frequency': float(record[4]) if record[4] else 0,
            'avg_duration': float(record[5]) if record[5] else 0,
            'avg_amplitude': float(record[6]) if record[6] else 0,
            'last_signal': record[7]
        })
    
    logger.info(f"Извлечено {len(telemetry_data)} записей телеметрии")
    context['task_instance'].xcom_push(key='telemetry_data', value=telemetry_data)
    #context['task_instance'].xcom_push(key='period_start', value=start_date.isoformat())
    #context['task_instance'].xcom_push(key='period_end', value=end_date.isoformat())
    
    return telemetry_data

def transform_and_aggregate(**context):
    """Трансформация данных"""
    logger.info("Трансформация данных...")
    
    users_data = context['task_instance'].xcom_pull(key='users_data', task_ids='extract_users_from_crm')
    telemetry_data = context['task_instance'].xcom_pull(key='telemetry_data', task_ids='extract_telemetry')
    period_start = context['task_instance'].xcom_pull(key='period_start', task_ids='extract_telemetry')
    period_end = context['task_instance'].xcom_pull(key='period_end', task_ids='extract_telemetry')
    
    if not telemetry_data:
        logger.warning("Нет данных телеметрии")
        return []
    
    # Создаем словарь пользователей для быстрого доступа
    users_dict = {u['user_id']: u for u in users_data}
    
    # Агрегация по пользователям
    user_agg = {}
    for telemetry in telemetry_data:
        user_id = telemetry['user_id']
        
        # Приводим все числовые значения к правильному типу
        signal_count = int(telemetry['signal_count']) if telemetry['signal_count'] else 0
        avg_frequency = float(telemetry['avg_frequency']) if telemetry['avg_frequency'] else 0.0
        avg_duration = float(telemetry['avg_duration']) if telemetry['avg_duration'] else 0.0
        avg_amplitude = float(telemetry['avg_amplitude']) if telemetry['avg_amplitude'] else 0.0

        if user_id not in user_agg:
            user_agg[user_id] = {
                'user_id': user_id,
                'prosthesis_types': set(),
                'muscle_groups': {},
                'total_signals': 0,
                'sum_frequency': 0,
                'sum_duration': 0,
                'sum_amplitude': 0,
                'last_signal': telemetry['last_signal']
            }
        
        user_agg[user_id]['prosthesis_types'].add(telemetry['prosthesis_type'])
        user_agg[user_id]['total_signals'] += signal_count
        user_agg[user_id]['sum_frequency'] += avg_frequency * signal_count
        user_agg[user_id]['sum_duration'] += avg_duration * signal_count
        user_agg[user_id]['sum_amplitude'] += avg_amplitude * signal_count
        
        muscle = telemetry['muscle_group']
        if muscle not in user_agg[user_id]['muscle_groups']:
            user_agg[user_id]['muscle_groups'][muscle] = 0
        user_agg[user_id]['muscle_groups'][muscle] += signal_count
    
    # Формируем финальные записи
    mart_data = []
    for user_id, agg in user_agg.items():
        if user_id not in users_dict:
            continue
        
        user = users_dict[user_id]
        total_signals = agg['total_signals']
        
        most_active_muscle = max(agg['muscle_groups'], key=agg['muscle_groups'].get) if agg['muscle_groups'] else 'unknown'
        
        mart_data.append({
            'user_id': user_id,
            'user_name': user['user_name'],
            'user_email': user['user_email'],
            'prosthesis_type': ', '.join(agg['prosthesis_types']),
            'prosthesis_serial_number': f"BP-{user_id:06d}",
            'total_signals': total_signals,
            'avg_signal_frequency': round(agg['sum_frequency'] / total_signals, 2) if total_signals > 0 else 0,
            'avg_signal_duration': round(agg['sum_duration'] / total_signals, 2) if total_signals > 0 else 0,
            'avg_signal_amplitude': round(agg['sum_amplitude'] / total_signals, 2) if total_signals > 0 else 0,
            'muscle_groups_used': list(agg['muscle_groups'].keys()),
            'most_active_muscle_group': most_active_muscle,
            'report_period_start': period_start,
            'report_period_end': period_end,
            'last_signal_time': agg['last_signal'],
            'report_generated_at': datetime.now().isoformat()
        })
    
    logger.info(f"Сформировано {len(mart_data)} записей")
    context['task_instance'].xcom_push(key='mart_data', value=mart_data)
    return mart_data

def load_to_reporting(**context):
    """Загрузка в витрину"""
    logger.info("Загрузка в ClickHouse...")
    
    mart_data = context['task_instance'].xcom_pull(key='mart_data', task_ids='transform_and_aggregate')
    
    if not mart_data:
        logger.warning("Нет данных для загрузки")
        return
    
    ch_hook = ClickHouseHTTPHook(host='olap_db', port=8123, timeout=30)
    
    # Загружаем данные
    for record in mart_data:
        # Экранируем строки
        user_name = record['user_name'].replace("'", "\\'")
        user_email = record['user_email'].replace("'", "\\'")
        prosthesis_type = record['prosthesis_type'].replace("'", "\\'")
        most_active_muscle = record['most_active_muscle_group'].replace("'", "\\'")
            
        # Преобразуем datetime значения в правильный формат для ClickHouse
        # ClickHouse ожидает формат: 'YYYY-MM-DD HH:MM:SS'
        report_period_start = record['report_period_start']
        report_period_end = record['report_period_end']
        last_signal_time = record['last_signal_time']
        report_generated_at = record['report_generated_at']
            
        # Если значения None или не определены, используем текущее время
        if not report_period_start:
            report_period_start = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        if not report_period_end:
            report_period_end = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        if not last_signal_time:
            last_signal_time = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        if not report_generated_at:
            report_generated_at = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
            
        # Если это объект datetime, преобразуем в строку
        if hasattr(report_period_start, 'strftime'):
            report_period_start = report_period_start.strftime('%Y-%m-%d %H:%M:%S')
        if hasattr(report_period_end, 'strftime'):
            report_period_end = report_period_end.strftime('%Y-%m-%d %H:%M:%S')
        if hasattr(last_signal_time, 'strftime'):
            last_signal_time = last_signal_time.strftime('%Y-%m-%d %H:%M:%S')
        if hasattr(report_generated_at, 'strftime'):
            report_generated_at = report_generated_at.strftime('%Y-%m-%d %H:%M:%S')
            
        insert_sql = f"""
                INSERT INTO bionicpro_analytics.reporting (
                    user_id,
                    user_name,
                    user_email,
                    prosthesis_type,
                    total_signals,
                    avg_signal_frequency,
                    avg_signal_duration,
                    avg_signal_amplitude,
                    report_period_start,
                    report_period_end
                ) VALUES (
                    {record['user_id']},
                    '{user_name}',
                    '{user_email}',
                    '{prosthesis_type}',
                    {record['total_signals']},
                    {record['avg_signal_frequency']},
                    {record['avg_signal_duration']},
                    {record['avg_signal_amplitude']},
                    '{report_period_start}',
                    '{report_period_end}'
                )
            """
        # insert_sql = f"""
        #         INSERT INTO bionicpro_analytics.reporting (
        #             user_id,
        #             user_name,
        #             user_email,
        #             prosthesis_type,
        #             prosthesis_serial_number,
        #             total_signals,
        #             avg_signal_frequency,
        #             avg_signal_duration,
        #             avg_signal_amplitude,
        #             muscle_groups_used,
        #             most_active_muscle_group,
        #             report_period_start,
        #             report_period_end,
        #             last_signal_time,
        #             report_generated_at
        #         ) VALUES (
        #             {record['user_id']},
        #             '{user_name}',
        #             '{user_email}',
        #             '{prosthesis_type}',
        #             '{prosthesis_serial_number}',
        #             {record['total_signals']},
        #             {record['avg_signal_frequency']},
        #             {record['avg_signal_duration']},
        #             {record['avg_signal_amplitude']},
        #             {record['muscle_groups_used']},
        #             '{most_active_muscle}',
        #             '{report_period_start}',
        #             '{report_period_end}',
        #             '{last_signal_time}',
        #             '{report_generated_at}'
        #         )
        #     """
        
        ch_hook.execute(insert_sql)
    
    logger.info(f"Загружено {len(mart_data)} записей")

# Определение задач
start_task = DummyOperator(task_id='start', dag=dag)

extract_users = PythonOperator(
    task_id='extract_users_from_crm',
    python_callable=extract_users_from_crm,
    dag=dag
)

extract_telemetry = PythonOperator(
    task_id='extract_telemetry',
    python_callable=extract_telemetry,
    dag=dag
)

transform_data = PythonOperator(
    task_id='transform_and_aggregate',
    python_callable=transform_and_aggregate,
    dag=dag
)

load_reporting = PythonOperator(
    task_id='load_to_reporting',
    python_callable=load_to_reporting,
    dag=dag
)

end_task = DummyOperator(task_id='end', dag=dag)

# Порядок выполнения
#start_task >> [extract_users, extract_telemetry] >> transform_data >> load_mart >> end_task
start_task >> [extract_users, extract_telemetry] >> transform_data >> load_reporting >>  end_task