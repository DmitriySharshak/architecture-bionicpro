import React, { useEffect, useState } from 'react';
import { ReactKeycloakProvider } from '@react-keycloak/web';
import Keycloak, { KeycloakConfig } from 'keycloak-js';
import ReportPage from './components/ReportPage';

const keycloakConfig: KeycloakConfig = {
  url: process.env.REACT_APP_KEYCLOAK_URL,
  realm: process.env.REACT_APP_KEYCLOAK_REALM||"",
  clientId: process.env.REACT_APP_KEYCLOAK_CLIENT_ID||""
};

const keycloak = new Keycloak(keycloakConfig);

// Дополнительная конфигурация для PKCE
const initOptions: Keycloak.KeycloakInitOptions = {
  onLoad: 'check-sso',
  pkceMethod: 'S256'
};

// Обработчики событий для отладки
const onKeycloakEvent = (event: unknown, error: unknown) => {
  if (event === 'onAuthSuccess') {
    console.log('PKCE Authentication successful');
    const verifier = sessionStorage.getItem('keycloak-pkce-verifier');
    if (verifier) {
      console.log('PKCE verifier found in storage');
    }
  }
};

const onKeycloakTokens = (tokens: unknown) => {
  console.log('Tokens refreshed/obtained');
};

const App: React.FC = () => {
  return (
    <ReactKeycloakProvider 
      authClient={keycloak} 
      initOptions={initOptions}
      onEvent={onKeycloakEvent}
      onTokens={onKeycloakTokens}>
      <div className="App">
        <ReportPage />
      </div>
    </ReactKeycloakProvider>
  );
};

export default App;