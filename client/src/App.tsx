import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Threats from './pages/Threats';
import UserBehavior from './pages/UserBehavior';
import ZeroDay from './pages/ZeroDay';
import Alerts from './pages/Alerts';
import Models from './pages/Models';
import Settings from './pages/Settings';

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/threats" element={<Threats />} />
        <Route path="/user-behavior" element={<UserBehavior />} />
        <Route path="/zero-day" element={<ZeroDay />} />
        <Route path="/alerts" element={<Alerts />} />
        <Route path="/models" element={<Models />} />
        <Route path="/settings" element={<Settings />} />
      </Routes>
    </Layout>
  );
}

export default App; 