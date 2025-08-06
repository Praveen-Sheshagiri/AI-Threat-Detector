import React, { useState } from 'react';
import { useQuery } from 'react-query';
import {
  MagnifyingGlassIcon,
  FunnelIcon,
  ShieldExclamationIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
} from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import clsx from 'clsx';

interface Threat {
  id: string;
  threatType: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  source: string;
  target: string;
  description: string;
  detectedAt: string;
  status: 'Active' | 'Investigating' | 'Mitigated' | 'Resolved' | 'FalsePositive';
  confidenceScore: number;
  isZeroDay: boolean;
}

const mockThreats: Threat[] = [
  {
    id: '1',
    threatType: 'Malware',
    severity: 'Critical',
    source: '192.168.1.100',
    target: 'web-server-01',
    description: 'Trojan detected attempting to establish backdoor connection',
    detectedAt: '2024-01-15T14:32:00Z',
    status: 'Active',
    confidenceScore: 0.95,
    isZeroDay: false,
  },
  {
    id: '2',
    threatType: 'DDoS',
    severity: 'High',
    source: 'Multiple IPs',
    target: 'api-gateway',
    description: 'Distributed denial of service attack targeting API endpoints',
    detectedAt: '2024-01-15T14:28:00Z',
    status: 'Investigating',
    confidenceScore: 0.87,
    isZeroDay: false,
  },
  {
    id: '3',
    threatType: 'Buffer Overflow',
    severity: 'Critical',
    source: '10.0.0.45',
    target: 'database-server',
    description: 'Potential zero-day buffer overflow exploit detected',
    detectedAt: '2024-01-15T14:25:00Z',
    status: 'Active',
    confidenceScore: 0.92,
    isZeroDay: true,
  },
  {
    id: '4',
    threatType: 'SQL Injection',
    severity: 'Medium',
    source: '203.0.113.5',
    target: 'user-portal',
    description: 'SQL injection attempt detected in login form',
    detectedAt: '2024-01-15T14:15:00Z',
    status: 'Mitigated',
    confidenceScore: 0.78,
    isZeroDay: false,
  },
];

export default function Threats() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedSeverity, setSelectedSeverity] = useState<string>('all');
  const [selectedStatus, setSelectedStatus] = useState<string>('all');

  const { data: threats, isLoading } = useQuery(
    ['threats', searchTerm, selectedSeverity, selectedStatus],
    async () => {
      // Mock API call - replace with actual API
      await new Promise(resolve => setTimeout(resolve, 500));
      return mockThreats.filter(threat => {
        const matchesSearch = threat.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                            threat.threatType.toLowerCase().includes(searchTerm.toLowerCase()) ||
                            threat.source.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesSeverity = selectedSeverity === 'all' || threat.severity === selectedSeverity;
        const matchesStatus = selectedStatus === 'all' || threat.status === selectedStatus;
        return matchesSearch && matchesSeverity && matchesStatus;
      });
    }
  );

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Critical': return 'bg-red-100 text-red-800 border-red-200';
      case 'High': return 'bg-orange-100 text-orange-800 border-orange-200';
      case 'Medium': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'Low': return 'bg-blue-100 text-blue-800 border-blue-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Active': return <ShieldExclamationIcon className="h-5 w-5 text-red-500" />;
      case 'Investigating': return <ClockIcon className="h-5 w-5 text-yellow-500" />;
      case 'Mitigated': return <CheckCircleIcon className="h-5 w-5 text-green-500" />;
      case 'Resolved': return <CheckCircleIcon className="h-5 w-5 text-gray-500" />;
      case 'FalsePositive': return <XCircleIcon className="h-5 w-5 text-gray-400" />;
      default: return null;
    }
  };

  const handleMitigateThreat = async (threatId: string) => {
    // Mock API call to mitigate threat
    console.log('Mitigating threat:', threatId);
  };

  const handleUpdateStatus = async (threatId: string, newStatus: string) => {
    // Mock API call to update threat status
    console.log('Updating threat status:', threatId, newStatus);
  };

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
        <h1 className="text-2xl font-bold text-gray-900">Threat Management</h1>
        <div className="flex items-center space-x-2">
          <span className="text-sm text-gray-600">
            {threats?.length || 0} threats detected
          </span>
        </div>
      </div>

      {/* Filters */}
      <div className="dashboard-card">
        <div className="flex flex-col sm:flex-row gap-4">
          {/* Search */}
          <div className="flex-1">
            <div className="relative">
              <MagnifyingGlassIcon className="h-5 w-5 absolute left-3 top-3 text-gray-400" />
              <input
                type="text"
                placeholder="Search threats..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>

          {/* Severity Filter */}
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              value={selectedSeverity}
              onChange={(e) => setSelectedSeverity(e.target.value)}
            >
              <option value="all">All Severities</option>
              <option value="Critical">Critical</option>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </div>

          {/* Status Filter */}
          <div>
            <select
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
            >
              <option value="all">All Statuses</option>
              <option value="Active">Active</option>
              <option value="Investigating">Investigating</option>
              <option value="Mitigated">Mitigated</option>
              <option value="Resolved">Resolved</option>
            </select>
          </div>
        </div>
      </div>

      {/* Threats Table */}
      <div className="dashboard-card">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Threat
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Severity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Source → Target
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Detected
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Confidence
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {threats?.map((threat) => (
                <tr key={threat.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div>
                        <div className="flex items-center space-x-2">
                          <div className="text-sm font-medium text-gray-900">
                            {threat.threatType}
                          </div>
                          {threat.isZeroDay && (
                            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                              Zero-Day
                            </span>
                          )}
                        </div>
                        <div className="text-sm text-gray-500">
                          {threat.description}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={clsx(
                      'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border',
                      getSeverityColor(threat.severity)
                    )}>
                      {threat.severity}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    <div className="flex items-center space-x-2">
                      <span>{threat.source}</span>
                      <span>→</span>
                      <span>{threat.target}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center space-x-2">
                      {getStatusIcon(threat.status)}
                      <span className="text-sm text-gray-900">{threat.status}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {format(new Date(threat.detectedAt), 'MMM dd, HH:mm')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="text-sm text-gray-900">
                        {(threat.confidenceScore * 100).toFixed(0)}%
                      </div>
                      <div className="ml-2 w-16 bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-primary-600 h-2 rounded-full"
                          style={{ width: `${threat.confidenceScore * 100}%` }}
                        ></div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                    {threat.status === 'Active' && (
                      <button
                        onClick={() => handleMitigateThreat(threat.id)}
                        className="text-red-600 hover:text-red-900"
                      >
                        Mitigate
                      </button>
                    )}
                    <button
                      onClick={() => handleUpdateStatus(threat.id, 'Investigating')}
                      className="text-primary-600 hover:text-primary-900"
                    >
                      Investigate
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
} 