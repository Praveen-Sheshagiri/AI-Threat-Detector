import React, { useState, useMemo } from 'react';

interface UserBehaviorRecord {
  id: string;
  userId: string;
  username: string;
  department: string;
  loginTime: string;
  ipAddress: string;
  location: string;
  deviceType: string;
  riskScore: number;
  activities: string[];
  anomalies: string[];
  status: 'normal' | 'suspicious' | 'critical';
}

const dummyUserBehaviorRecords: UserBehaviorRecord[] = [
  {
    id: '1',
    userId: 'USR001',
    username: 'john.doe',
    department: 'Engineering',
    loginTime: '2025-08-06 09:15:23',
    ipAddress: '192.168.1.45',
    location: 'New York, NY',
    deviceType: 'Windows Laptop',
    riskScore: 15,
    activities: ['File access', 'Email sent', 'Database query'],
    anomalies: [],
    status: 'normal'
  },
  {
    id: '2',
    userId: 'USR002',
    username: 'sarah.wilson',
    department: 'Marketing',
    loginTime: '2025-08-06 08:45:12',
    ipAddress: '192.168.1.23',
    location: 'Los Angeles, CA',
    deviceType: 'MacBook Pro',
    riskScore: 72,
    activities: ['Multiple failed logins', 'Unusual file download', 'VPN access'],
    anomalies: ['Login from new location', 'Off-hours activity'],
    status: 'suspicious'
  },
  {
    id: '3',
    userId: 'USR003',
    username: 'mike.johnson',
    department: 'Finance',
    loginTime: '2025-08-06 07:30:45',
    ipAddress: '10.0.0.156',
    location: 'Chicago, IL',
    deviceType: 'Windows Desktop',
    riskScore: 8,
    activities: ['Spreadsheet access', 'Report generation', 'Email reading'],
    anomalies: [],
    status: 'normal'
  },
  {
    id: '4',
    userId: 'USR004',
    username: 'alex.chen',
    department: 'IT Security',
    loginTime: '2025-08-06 10:22:18',
    ipAddress: '172.16.0.89',
    location: 'Seattle, WA',
    deviceType: 'Linux Workstation',
    riskScore: 95,
    activities: ['Admin panel access', 'Multiple system queries', 'Large data transfer'],
    anomalies: ['Privilege escalation attempt', 'Suspicious network activity', 'Data exfiltration pattern'],
    status: 'critical'
  },
  {
    id: '5',
    userId: 'USR005',
    username: 'emma.davis',
    department: 'HR',
    loginTime: '2025-08-06 09:05:33',
    ipAddress: '192.168.2.67',
    location: 'Boston, MA',
    deviceType: 'iPad Pro',
    riskScore: 25,
    activities: ['Document review', 'Calendar access', 'Video call'],
    anomalies: ['Unusual device'],
    status: 'normal'
  },
  {
    id: '6',
    userId: 'USR006',
    username: 'david.brown',
    department: 'Sales',
    loginTime: '2025-08-06 11:15:07',
    ipAddress: '203.0.113.45',
    location: 'London, UK',
    deviceType: 'iPhone',
    riskScore: 68,
    activities: ['CRM access', 'Client data download', 'International transfer'],
    anomalies: ['Login from foreign country', 'Mobile device login'],
    status: 'suspicious'
  }
];

export default function UserBehavior() {
  const [selectedUser, setSelectedUser] = useState<UserBehaviorRecord | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>('all');

  const filteredRecords = useMemo(() => 
    dummyUserBehaviorRecords.filter(record => 
      filterStatus === 'all' || record.status === filterStatus
    ), [filterStatus]
  );

  const getRiskColor = (score: number) => {
    if (score < 30) return 'text-green-600 bg-green-100';
    if (score < 70) return 'text-yellow-600 bg-yellow-100';
    return 'text-red-600 bg-red-100';
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'normal': return 'text-green-600 bg-green-100';
      case 'suspicious': return 'text-yellow-600 bg-yellow-100';
      case 'critical': return 'text-red-600 bg-red-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">User Behavior Analysis</h1>
        
        <div className="flex space-x-4">
          <select 
            value={filterStatus} 
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value="normal">Normal</option>
            <option value="suspicious">Suspicious</option>
            <option value="critical">Critical</option>
          </select>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Total Users</h3>
          <p className="text-3xl font-bold text-blue-600">{dummyUserBehaviorRecords.length}</p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Normal Behavior</h3>
          <p className="text-3xl font-bold text-green-600">
            {dummyUserBehaviorRecords.filter(r => r.status === 'normal').length}
          </p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Suspicious Activity</h3>
          <p className="text-3xl font-bold text-yellow-600">
            {dummyUserBehaviorRecords.filter(r => r.status === 'suspicious').length}
          </p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Critical Alerts</h3>
          <p className="text-3xl font-bold text-red-600">
            {dummyUserBehaviorRecords.filter(r => r.status === 'critical').length}
          </p>
        </div>
      </div>

      {/* User Behavior Table */}
      <div className="dashboard-card">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">User Activity Overview</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Department</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Login Time</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Location</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Risk Score</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredRecords.map((record) => (
                <tr key={record.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">{record.username}</div>
                      <div className="text-sm text-gray-500">{record.userId}</div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{record.department}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{record.loginTime}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm text-gray-900">{record.location}</div>
                      <div className="text-sm text-gray-500">{record.ipAddress}</div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getRiskColor(record.riskScore)}`}>
                      {record.riskScore}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full capitalize ${getStatusColor(record.status)}`}>
                      {record.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button
                      onClick={() => setSelectedUser(record)}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      View Details
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* User Details Modal */}
      {selectedUser && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-11/12 md:w-3/4 lg:w-1/2 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-medium text-gray-900">User Behavior Details</h3>
                <button
                  onClick={() => setSelectedUser(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <span className="sr-only">Close</span>
                  ✕
                </button>
              </div>
              
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Username</label>
                    <p className="text-sm text-gray-900">{selectedUser.username}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Department</label>
                    <p className="text-sm text-gray-900">{selectedUser.department}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Device Type</label>
                    <p className="text-sm text-gray-900">{selectedUser.deviceType}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Risk Score</label>
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getRiskColor(selectedUser.riskScore)}`}>
                      {selectedUser.riskScore}
                    </span>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Recent Activities</label>
                  <ul className="list-disc list-inside space-y-1">
                    {selectedUser.activities.map((activity, index) => (
                      <li key={index} className="text-sm text-gray-600">{activity}</li>
                    ))}
                  </ul>
                </div>

                {selectedUser.anomalies.length > 0 && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Detected Anomalies</label>
                    <ul className="list-disc list-inside space-y-1">
                      {selectedUser.anomalies.map((anomaly, index) => (
                        <li key={index} className="text-sm text-red-600">{anomaly}</li>
                      ))}
                    </ul>
                  </div>
                )}

                <div className="flex justify-end space-x-3 mt-6">
                  <button
                    onClick={() => setSelectedUser(null)}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300"
                  >
                    Close
                  </button>
                  {selectedUser.status !== 'normal' && (
                    <button className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700">
                      Investigate
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
} 