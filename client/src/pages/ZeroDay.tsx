import React, { useState } from 'react';

interface ZeroDayVulnerability {
  id: string;
  cveId: string | null;
  title: string;
  description: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  cvssScore: number;
  detectedAt: string;
  affectedSystems: string[];
  exploitability: 'active' | 'poc' | 'theoretical';
  status: 'new' | 'investigating' | 'patching' | 'mitigated' | 'resolved';
  threatActors: string[];
  indicators: string[];
  mitigationSteps: string[];
  lastUpdated: string;
  source: string;
}

const dummyZeroDayVulnerabilities: ZeroDayVulnerability[] = [
  {
    id: '1',
    cveId: null,
    title: 'Remote Code Execution in Web Framework',
    description: 'Newly discovered zero-day vulnerability allowing remote code execution through malformed HTTP requests in popular web framework.',
    severity: 'critical',
    cvssScore: 9.8,
    detectedAt: '2025-08-06 14:23:15',
    affectedSystems: ['Web Server Farm', 'API Gateway', 'Load Balancer'],
    exploitability: 'active',
    status: 'investigating',
    threatActors: ['APT-29', 'Unknown Group'],
    indicators: ['Unusual HTTP patterns', 'Memory corruption signatures', 'Privilege escalation attempts'],
    mitigationSteps: [
      'Block suspicious HTTP patterns',
      'Enable additional logging',
      'Implement temporary WAF rules',
      'Monitor for exploitation attempts'
    ],
    lastUpdated: '2025-08-06 15:45:32',
    source: 'Internal ML Detection'
  },
  {
    id: '2',
    cveId: 'CVE-2025-0001',
    title: 'Buffer Overflow in Network Driver',
    description: 'Zero-day buffer overflow vulnerability in network driver allowing kernel-level code execution.',
    severity: 'high',
    cvssScore: 8.4,
    detectedAt: '2025-08-05 22:15:44',
    affectedSystems: ['Windows Workstations', 'Server Infrastructure'],
    exploitability: 'poc',
    status: 'patching',
    threatActors: ['Lazarus Group'],
    indicators: ['Network driver crashes', 'Kernel memory corruption', 'Unusual network traffic'],
    mitigationSteps: [
      'Deploy emergency patch',
      'Restrict network access',
      'Monitor driver stability',
      'Update network configurations'
    ],
    lastUpdated: '2025-08-06 09:12:18',
    source: 'Threat Intelligence Feed'
  },
  {
    id: '3',
    cveId: null,
    title: 'SQL Injection Bypass in Database Engine',
    description: 'Novel SQL injection technique bypassing existing protections in enterprise database engine.',
    severity: 'high',
    cvssScore: 8.1,
    detectedAt: '2025-08-04 16:33:27',
    affectedSystems: ['Database Cluster', 'Data Warehouse', 'Analytics Platform'],
    exploitability: 'theoretical',
    status: 'mitigated',
    threatActors: ['Unknown'],
    indicators: ['Malformed SQL queries', 'Database error patterns', 'Data exfiltration attempts'],
    mitigationSteps: [
      'Update SQL filters',
      'Enhanced query validation',
      'Database access restrictions',
      'Audit database connections'
    ],
    lastUpdated: '2025-08-06 11:28:45',
    source: 'Security Research'
  },
  {
    id: '4',
    cveId: 'CVE-2025-0002',
    title: 'Authentication Bypass in SSO System',
    description: 'Critical authentication bypass vulnerability in single sign-on system allowing unauthorized access.',
    severity: 'critical',
    cvssScore: 9.3,
    detectedAt: '2025-08-03 13:47:11',
    affectedSystems: ['SSO Portal', 'Identity Management', 'All Connected Services'],
    exploitability: 'active',
    status: 'resolved',
    threatActors: ['APT-40', 'FIN7'],
    indicators: ['Failed authentication logs', 'Session token anomalies', 'Unauthorized access attempts'],
    mitigationSteps: [
      'Emergency SSO patch deployed',
      'Force password resets',
      'Revoke all active sessions',
      'Enable MFA enforcement'
    ],
    lastUpdated: '2025-08-06 08:15:22',
    source: 'Vendor Advisory'
  },
  {
    id: '5',
    cveId: null,
    title: 'Privilege Escalation in Container Runtime',
    description: 'Zero-day vulnerability in container runtime allowing privilege escalation to host system.',
    severity: 'medium',
    cvssScore: 6.7,
    detectedAt: '2025-08-02 19:22:33',
    affectedSystems: ['Container Infrastructure', 'Kubernetes Clusters', 'Docker Hosts'],
    exploitability: 'poc',
    status: 'new',
    threatActors: ['Unknown'],
    indicators: ['Container breakout attempts', 'Host filesystem access', 'Privilege escalation patterns'],
    mitigationSteps: [
      'Container isolation review',
      'Runtime security policies',
      'Host monitoring enhancement',
      'Container image scanning'
    ],
    lastUpdated: '2025-08-06 07:45:18',
    source: 'Bug Bounty Program'
  },
  {
    id: '6',
    cveId: 'CVE-2025-0003',
    title: 'Memory Corruption in Email Gateway',
    description: 'Memory corruption vulnerability in email gateway allowing remote code execution through crafted emails.',
    severity: 'high',
    cvssScore: 7.9,
    detectedAt: '2025-08-01 11:15:55',
    affectedSystems: ['Email Gateway', 'Mail Servers', 'Spam Filters'],
    exploitability: 'active',
    status: 'investigating',
    threatActors: ['APT-1', 'Cozy Bear'],
    indicators: ['Email parsing errors', 'Gateway crashes', 'Malicious email attachments'],
    mitigationSteps: [
      'Email filtering enhancement',
      'Gateway isolation',
      'Attachment scanning boost',
      'Emergency response protocol'
    ],
    lastUpdated: '2025-08-06 13:30:41',
    source: 'Honeypot Detection'
  }
];

export default function ZeroDay() {
  const [selectedVulnerability, setSelectedVulnerability] = useState<ZeroDayVulnerability | null>(null);
  const [filterSeverity, setFilterSeverity] = useState<string>('all');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  const filteredVulnerabilities = dummyZeroDayVulnerabilities.filter(vuln => 
    (filterSeverity === 'all' || vuln.severity === filterSeverity) &&
    (filterStatus === 'all' || vuln.status === filterStatus)
  );

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return 'text-red-800 bg-red-100 border-red-200';
      case 'high': return 'text-red-600 bg-red-50 border-red-200';
      case 'medium': return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'low': return 'text-green-600 bg-green-50 border-green-200';
      default: return 'text-gray-600 bg-gray-50 border-gray-200';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'new': return 'text-blue-600 bg-blue-100';
      case 'investigating': return 'text-yellow-600 bg-yellow-100';
      case 'patching': return 'text-purple-600 bg-purple-100';
      case 'mitigated': return 'text-orange-600 bg-orange-100';
      case 'resolved': return 'text-green-600 bg-green-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const getExploitabilityColor = (exploitability: string) => {
    switch (exploitability) {
      case 'active': return 'text-red-600 bg-red-100';
      case 'poc': return 'text-yellow-600 bg-yellow-100';
      case 'theoretical': return 'text-green-600 bg-green-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Zero-Day Vulnerabilities</h1>
        
        <div className="flex space-x-4">
          <select 
            value={filterSeverity} 
            onChange={(e) => setFilterSeverity(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Severities</option>
            <option value="critical">Critical</option>
            <option value="high">High</option>
            <option value="medium">Medium</option>
            <option value="low">Low</option>
          </select>
          
          <select 
            value={filterStatus} 
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value="new">New</option>
            <option value="investigating">Investigating</option>
            <option value="patching">Patching</option>
            <option value="mitigated">Mitigated</option>
            <option value="resolved">Resolved</option>
          </select>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-5 gap-6">
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Total Vulnerabilities</h3>
          <p className="text-3xl font-bold text-blue-600">{dummyZeroDayVulnerabilities.length}</p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Critical</h3>
          <p className="text-3xl font-bold text-red-600">
            {dummyZeroDayVulnerabilities.filter(v => v.severity === 'critical').length}
          </p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">High</h3>
          <p className="text-3xl font-bold text-orange-600">
            {dummyZeroDayVulnerabilities.filter(v => v.severity === 'high').length}
          </p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Active Exploits</h3>
          <p className="text-3xl font-bold text-red-600">
            {dummyZeroDayVulnerabilities.filter(v => v.exploitability === 'active').length}
          </p>
        </div>
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-gray-900">Unresolved</h3>
          <p className="text-3xl font-bold text-yellow-600">
            {dummyZeroDayVulnerabilities.filter(v => v.status !== 'resolved').length}
          </p>
        </div>
      </div>

      {/* Vulnerabilities Table */}
      <div className="dashboard-card">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Zero-Day Vulnerability Tracking</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Vulnerability</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Severity</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">CVSS Score</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Exploitability</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Detected</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredVulnerabilities.map((vulnerability) => (
                <tr key={vulnerability.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">{vulnerability.title}</div>
                      <div className="text-sm text-gray-500">
                        {vulnerability.cveId || 'No CVE assigned'}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full border capitalize ${getSeverityColor(vulnerability.severity)}`}>
                      {vulnerability.severity}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`text-sm font-semibold ${vulnerability.cvssScore >= 9 ? 'text-red-600' : vulnerability.cvssScore >= 7 ? 'text-orange-600' : 'text-yellow-600'}`}>
                      {vulnerability.cvssScore}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full capitalize ${getExploitabilityColor(vulnerability.exploitability)}`}>
                      {vulnerability.exploitability}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full capitalize ${getStatusColor(vulnerability.status)}`}>
                      {vulnerability.status.replace('_', ' ')}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {new Date(vulnerability.detectedAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button
                      onClick={() => setSelectedVulnerability(vulnerability)}
                      className="text-blue-600 hover:text-blue-900 mr-3"
                    >
                      View Details
                    </button>
                    {vulnerability.status !== 'resolved' && (
                      <button className="text-red-600 hover:text-red-900">
                        Escalate
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Vulnerability Details Modal */}
      {selectedVulnerability && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-10 mx-auto p-5 border w-11/12 md:w-3/4 lg:w-2/3 shadow-lg rounded-md bg-white max-h-screen overflow-y-auto">
            <div className="mt-3">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-medium text-gray-900">Zero-Day Vulnerability Details</h3>
                <button
                  onClick={() => setSelectedVulnerability(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <span className="sr-only">Close</span>
                  ✕
                </button>
              </div>
              
              <div className="space-y-6">
                {/* Basic Information */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Title</label>
                    <p className="text-sm text-gray-900 font-semibold">{selectedVulnerability.title}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">CVE ID</label>
                    <p className="text-sm text-gray-900">{selectedVulnerability.cveId || 'Not assigned'}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Severity</label>
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full border capitalize ${getSeverityColor(selectedVulnerability.severity)}`}>
                      {selectedVulnerability.severity}
                    </span>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">CVSS Score</label>
                    <p className="text-sm font-semibold text-gray-900">{selectedVulnerability.cvssScore}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Source</label>
                    <p className="text-sm text-gray-900">{selectedVulnerability.source}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Last Updated</label>
                    <p className="text-sm text-gray-900">{selectedVulnerability.lastUpdated}</p>
                  </div>
                </div>

                {/* Description */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                  <p className="text-sm text-gray-600 bg-gray-50 p-3 rounded">{selectedVulnerability.description}</p>
                </div>

                {/* Affected Systems */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Affected Systems</label>
                  <div className="flex flex-wrap gap-2">
                    {selectedVulnerability.affectedSystems.map((system, index) => (
                      <span key={index} className="inline-flex px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded">
                        {system}
                      </span>
                    ))}
                  </div>
                </div>

                {/* Threat Actors */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Associated Threat Actors</label>
                  <div className="flex flex-wrap gap-2">
                    {selectedVulnerability.threatActors.map((actor, index) => (
                      <span key={index} className="inline-flex px-2 py-1 text-xs font-medium bg-red-100 text-red-800 rounded">
                        {actor}
                      </span>
                    ))}
                  </div>
                </div>

                {/* Indicators */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Indicators of Compromise</label>
                  <ul className="list-disc list-inside space-y-1">
                    {selectedVulnerability.indicators.map((indicator, index) => (
                      <li key={index} className="text-sm text-gray-600">{indicator}</li>
                    ))}
                  </ul>
                </div>

                {/* Mitigation Steps */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Mitigation Steps</label>
                  <ul className="list-decimal list-inside space-y-1">
                    {selectedVulnerability.mitigationSteps.map((step, index) => (
                      <li key={index} className="text-sm text-gray-600">{step}</li>
                    ))}
                  </ul>
                </div>

                <div className="flex justify-end space-x-3 mt-6">
                  <button
                    onClick={() => setSelectedVulnerability(null)}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300"
                  >
                    Close
                  </button>
                  <button className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700">
                    Export Report
                  </button>
                  {selectedVulnerability.status !== 'resolved' && (
                    <button className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700">
                      Emergency Response
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