import React, { useState } from 'react';
import { useKeycloak } from '@react-keycloak/web';

 // Интерфейсы для данных отчета
interface Report {
  userName: string; 
  userEmail: string; 
  reportDate: string;
  avgSignalStrength: number;
  prosthesisType: string;
  avgSignalFrequency: number;
  avgSignalDuration: number;
  avgSignalAmplitude: number;
}

interface ReportResponse {
  reports: Report[];
  period_start: string;
  period_end: string;
}

const ReportPage: React.FC = () => {
  const { keycloak, initialized } = useKeycloak();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reports, setReports] = useState<Report[]>([]);
  
  // Дата-фильтр
  const [startDate, setStartDate] = useState(() => {
    const date = new Date();
    date.setDate(date.getDate() - 30);
    return date.toISOString().split('T')[0];
  });
  const [endDate, setEndDate] = useState(() => new Date().toISOString().split('T')[0]);

  const downloadReport = async () => {
    if (!keycloak?.token) {
      setError('Not authenticated');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const response = await fetch(`${process.env.REACT_APP_API_URL}/reports?startdate=${startDate}&enddate=${endDate}`, {
        headers: {
          'Authorization': `Bearer ${keycloak.token}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.status === 403) {
        throw new Error('Ошибка доступа: Вы не можете получить отчеты');
      }

      if (!response.ok) {
        throw new Error(`ошибка получения отчетов: ${response.statusText}`);
      }

      
      //const data: ReportResponse = await response.json();
      const data = await response.json();
      if (data?.success && data?.data?.reports) {
        setReports(data.data.reports);
      } else {
        setReports([]);
      }
      
      // if (data.reports.length === 0) {
      //   setError('не найдены отчеты за выбранный период');
      // }

      console.info(data)
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  if (!initialized) {
    return <div>Loading...</div>;
  }

  if (!keycloak.authenticated) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <button
          onClick={() => keycloak.login()}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Login
        </button>
      </div>
    );
  }

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <h1>BionicPRO Reports</h1>
      <p>User: {keycloak.tokenParsed?.preferred_username}</p>
      
      {/* Фильтр по датам */}
      <div style={{ 
        background: '#f5f5f5', 
        padding: '15px', 
        borderRadius: '8px', 
        marginBottom: '20px',
        display: 'flex',
        gap: '15px',
        alignItems: 'flex-end'
      }}>
        <div>
          <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>
            Начало периода:
          </label>
          <input
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
          />
        </div>
        <div>
          <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>
            Окончание периода:
          </label>
          <input
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
          />
        </div>
        <button
          onClick={downloadReport}
          disabled={loading}
          style={{
            padding: '8px 16px',
            background: '#1061B0',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          {loading ? 'Загрузка...' : 'Применить фильтр'}
        </button>
      </div>
      
      {error && (
        <div style={{ color: 'red', padding: '10px', background: '#ffebee', marginBottom: '20px' }}>
          {error}
        </div>
      )}
      
      {loading && <p>Загрузка...</p>}
      
      {reports.length === 0 && !loading && (
        <p>Отчеты за выбранный период отсутствуют</p>
      )}
      
      {reports.length > 0 && (
        <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '20px' }}>
          <thead>
            <tr style={{ background: '#1061B0', color: 'white' }}>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Время</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Имя пользователя</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Почта</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Тип протеза</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Средняя частота сигнала (Гц)</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Средняя длительность (мс)</th>
              <th style={{ padding: '10px', border: '1px solid #ddd' }}>Средняя амплитуда (мкВ)</th>
            </tr>
          </thead>
          <tbody>
            {reports.map((report, idx) => (
              <tr key={idx} style={{ borderBottom: '1px solid #ddd' }}>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.reportDate}</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.userName}</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.userEmail}</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.prosthesisType}</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.avgSignalFrequency} ms</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.avgSignalDuration}</td>
                <td style={{ padding: '10px', textAlign: 'center' }}>{report.avgSignalAmplitude}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );  
};

export default ReportPage;