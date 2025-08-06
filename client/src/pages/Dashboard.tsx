import React, { useState, useEffect } from 'react';
import { useQuery } from 'react-query';
import {
  ShieldExclamationIcon,
  UserGroupIcon,
  BugAntIcon,
  BellAlertIcon,
  ArrowUpIcon,
  ArrowDownIcon,
} from '@heroicons/react/24/outline';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, PieChart, Pie, Cell } from 'recharts';
import * as signalR from '@microsoft/signalr';
import { format } from 'date-fns';
import toast from 'react-hot-toast';

interface DashboardMetrics {
  activeThreats: number;
  zeroDayVulnerabilities: number;
  behaviorAnomalies: number;
  activeAlerts: number;
  threatTrend: number;
  systemHealth: 'healthy' | 'warning' | 'critical';
}

interface ThreatData {
  timestamp: string;
  count: number;
  severity: 'low' | 'medium' | 'high' | 'critical';
}

const mockThreatData: ThreatData[] = [
  { timestamp: '00:00', count: 12, severity: 'low' },
  { timestamp: '04:00', count: 8, severity: 'medium' },
  { timestamp: '08:00', count: 15, severity: 'high' },
  { timestamp: '12:00', count: 23, severity: 'critical' },
  { timestamp: '16:00', count: 18, severity: 'high' },
  { timestamp: '20:00', count: 7, severity: 'low' },
];

const threatDistribution = [
  { name: 'Malware', value: 35, color: '#ef4444' },
  { name: 'DDoS', value: 25, color: '#f97316' },
  { name: 'Intrusion', value: 20, color: '#eab308' },
  { name: 'Phishing', value: 15, color: '#3b82f6' },
  { name: 'Other', value: 5, color: '#6b7280' },
];

export default function Dashboard() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [realtimeMetrics, setRealtimeMetrics] = useState<DashboardMetrics>({
    activeThreats: 47,
    zeroDayVulnerabilities: 3,
    behaviorAnomalies: 12,
    activeAlerts: 8,
    threatTrend: 15.3,
    systemHealth: 'healthy',
  });

  const { data: dashboardData, isLoading } = useQuery(
    'dashboard-metrics',
    async () => {
      // Mock API call - replace with actual API
      await new Promise(resolve => setTimeout(resolve, 1000));
      return realtimeMetrics;
    },
    {
      refetchInterval: 30000, // Refetch every 30 seconds
    }
  );

  useEffect(() => {
    // Set up SignalR connection for real-time updates
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('/threatHub')
      .build();

    setConnection(newConnection);

    newConnection.start()
      .then(() => {
        console.log('SignalR Connected');
        
        // Listen for threat detection events
        newConnection.on('ThreatDetected', (threat) => {
          toast.error(`New threat detected: ${threat.threatType}`);
          setRealtimeMetrics(prev => ({
            ...prev,
            activeThreats: prev.activeThreats + 1
          }));
        });

        // Listen for security alerts
        newConnection.on('SecurityAlert', (alert) => {
          toast(`Security Alert: ${alert.title}`, {
            icon: '🚨',
          });
          setRealtimeMetrics(prev => ({
            ...prev,
            activeAlerts: prev.activeAlerts + 1
          }));
        });

        // Listen for user behavior anomalies
        newConnection.on('UserBehaviorAnomaly', (anomaly) => {
          toast.error(`Behavior anomaly detected for user: ${anomaly.userId}`);
          setRealtimeMetrics(prev => ({
            ...prev,
            behaviorAnomalies: prev.behaviorAnomalies + 1
          }));
        });

        // Listen for zero-day vulnerabilities
        newConnection.on('ZeroDayVulnerability', (vulnerability) => {
          toast.error(`Zero-day vulnerability: ${vulnerability.vulnerabilityType}`);
          setRealtimeMetrics(prev => ({
            ...prev,
            zeroDayVulnerabilities: prev.zeroDayVulnerabilities + 1
          }));
        });
      })
      .catch(error => console.error('SignalR Connection Error:', error));

    return () => {
      newConnection.stop();
    };
  }, []);

  const metricCards = [
    {
      title: 'Active Threats',
      value: realtimeMetrics.activeThreats,
      icon: ShieldExclamationIcon,
      trend: realtimeMetrics.threatTrend,
      color: 'text-red-600',
      bgColor: 'bg-red-50',
      borderColor: 'border-red-200',
    },
    {
      title: 'Zero-Day Vulnerabilities',
      value: realtimeMetrics.zeroDayVulnerabilities,
      icon: BugAntIcon,
      trend: -2.5,
      color: 'text-orange-600',
      bgColor: 'bg-orange-50',
      borderColor: 'border-orange-200',
    },
    {
      title: 'Behavior Anomalies',
      value: realtimeMetrics.behaviorAnomalies,
      icon: UserGroupIcon,
      trend: 8.2,
      color: 'text-yellow-600',
      bgColor: 'bg-yellow-50',
      borderColor: 'border-yellow-200',
    },
    {
      title: 'Active Alerts',
      value: realtimeMetrics.activeAlerts,
      icon: BellAlertIcon,
      trend: -5.1,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      borderColor: 'border-blue-200',
    },
  ];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="loading-spinner"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Threat Detection Dashboard</h1>
        <div className="flex items-center space-x-2">
          <div className={`h-3 w-3 rounded-full ${
            realtimeMetrics.systemHealth === 'healthy' ? 'bg-green-400' :
            realtimeMetrics.systemHealth === 'warning' ? 'bg-yellow-400' : 'bg-red-400'
          } animate-pulse`}></div>
          <span className="text-sm text-gray-600">
            System {realtimeMetrics.systemHealth}
          </span>
        </div>
      </div>

      {/* Metrics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {metricCards.map((metric, index) => (
          <div
            key={metric.title}
            className={`metric-card ${metric.bgColor} ${metric.borderColor} border-l-4`}
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">{metric.title}</p>
                <p className="text-2xl font-bold text-gray-900">{metric.value}</p>
                <div className="flex items-center mt-1">
                  {metric.trend > 0 ? (
                    <ArrowUpIcon className="h-4 w-4 text-red-500" />
                  ) : (
                    <ArrowDownIcon className="h-4 w-4 text-green-500" />
                  )}
                  <span className={`text-sm ml-1 ${
                    metric.trend > 0 ? 'text-red-600' : 'text-green-600'
                  }`}>
                    {Math.abs(metric.trend)}%
                  </span>
                  <span className="text-sm text-gray-500 ml-1">vs last hour</span>
                </div>
              </div>
              <div className={`p-3 rounded-lg ${metric.bgColor}`}>
                <metric.icon className={`h-6 w-6 ${metric.color}`} />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Threat Timeline */}
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Threat Activity (24h)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={mockThreatData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="timestamp" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="count" stroke="#3b82f6" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Threat Distribution */}
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Threat Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={threatDistribution}
                cx="50%"
                cy="50%"
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
                label={({ name, value }) => `${name}: ${value}%`}
              >
                {threatDistribution.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Security Events</h3>
        <div className="space-y-3">
          {[
            {
              time: '14:32',
              type: 'Threat Detection',
              message: 'Malware detected from IP 192.168.1.100',
              severity: 'high',
            },
            {
              time: '14:28',
              type: 'User Behavior',
              message: 'Anomalous login pattern for user john.doe',
              severity: 'medium',
            },
            {
              time: '14:25',
              type: 'Zero-Day',
              message: 'Potential zero-day exploit in web application',
              severity: 'critical',
            },
            {
              time: '14:20',
              type: 'System Alert',
              message: 'High CPU usage detected on threat detection system',
              severity: 'low',
            },
          ].map((event, index) => (
            <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className={`w-2 h-2 rounded-full ${
                  event.severity === 'critical' ? 'bg-red-500' :
                  event.severity === 'high' ? 'bg-orange-500' :
                  event.severity === 'medium' ? 'bg-yellow-500' : 'bg-blue-500'
                }`}></div>
                <div>
                  <p className="text-sm font-medium text-gray-900">{event.message}</p>
                  <p className="text-xs text-gray-500">{event.type}</p>
                </div>
              </div>
              <span className="text-sm text-gray-500">{event.time}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
} 