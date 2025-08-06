# AI Threat Detector

An advanced AI-powered threat detection and security monitoring system built with .NET Core, React, and AWS integration. This system provides real-time threat detection, zero-day vulnerability identification, user behavior analysis, and continuous learning capabilities.

## 🚀 Features

### Core Security Features
- **Real-time Threat Detection**: ML.NET-powered threat detection with continuous monitoring
- **Zero-Day Vulnerability Detection**: Specialized algorithms to identify unknown threats
- **User Behavior Analysis**: Anomaly detection for unusual user activities
- **Continuous Learning**: Adaptive ML models that improve over time
- **Real-time Alerts**: Instant notifications via SignalR
- **Dynamic Security Measures**: Automatic threat response and mitigation

### Technical Features
- **Scalable Architecture**: Microservices-based design with clean architecture
- **Real-time Communication**: SignalR for live updates and notifications
- **Modern UI**: Responsive React dashboard with Tailwind CSS
- **Cloud-Ready**: AWS integration for scalable deployment
- **Comprehensive API**: RESTful APIs for all security operations
- **Background Processing**: Automated threat monitoring and model retraining

## 🏗️ Architecture

### Backend (.NET Core)
```
src/
├── ThreatDetector.API/          # Web API controllers and SignalR hubs
├── ThreatDetector.Core/         # Domain models and interfaces
├── ThreatDetector.ML/           # ML.NET models and services
└── ThreatDetector.Data/         # Entity Framework data layer
```

### Frontend (React)
```
client/
├── src/
│   ├── components/              # Reusable UI components
│   ├── pages/                   # Application pages
│   ├── services/                # API service layers
│   └── utils/                   # Utility functions
└── public/                      # Static assets
```

## 🛠️ Technology Stack

### Backend
- **.NET Core 8.0**: High-performance web API framework
- **ML.NET**: Machine learning framework for threat detection
- **Entity Framework Core**: Object-relational mapping
- **SignalR**: Real-time web functionality
- **Serilog**: Structured logging
- **SQL Server**: Database for threat data storage

### Frontend
- **React 18**: Modern UI library with hooks
- **TypeScript**: Type-safe JavaScript development
- **Tailwind CSS**: Utility-first CSS framework
- **React Query**: Data fetching and caching
- **Recharts**: Data visualization library
- **React Router**: Client-side routing

### Cloud & Infrastructure
- **AWS RDS**: Managed database service
- **AWS S3**: Object storage for ML models
- **AWS CloudWatch**: Monitoring and logging
- **AWS Lambda**: Serverless compute
- **AWS Kinesis**: Real-time data streaming

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB for development)
- [Git](https://git-scm.com/)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd AI-Threat-Detector
   ```

2. **Restore .NET packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   Edit `src/ThreatDetector.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ThreatDetectorDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Create and seed database**
   ```bash
   cd src/ThreatDetector.API
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:7001`

### Frontend Setup

1. **Navigate to client directory**
   ```bash
   cd client
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm start
   ```
   The React app will be available at `http://localhost:3000`

## 📊 Dashboard Features

### Main Dashboard
- **Real-time Metrics**: Active threats, vulnerabilities, and alerts
- **Threat Timeline**: 24-hour threat activity visualization
- **Threat Distribution**: Pie chart of threat types
- **Recent Activity**: Live feed of security events
- **System Health**: Overall system status monitoring

### Threat Management
- **Threat List**: Comprehensive view of all detected threats
- **Filtering & Search**: Advanced threat filtering capabilities
- **Threat Details**: Detailed analysis of each threat
- **Mitigation Actions**: Manual and automated threat response
- **Status Tracking**: Threat lifecycle management

### Additional Modules
- **User Behavior Analysis**: Monitor and analyze user activities
- **Zero-Day Detection**: Manage unknown vulnerability threats
- **Security Alerts**: Centralized alert management
- **ML Model Management**: Monitor and retrain AI models
- **System Settings**: Configure detection parameters

## 🔧 Configuration

### API Configuration
Key settings in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables
Set these for production deployment:
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
ConnectionStrings__DefaultConnection=<production-connection-string>
```

## 🚀 Deployment

### Docker Deployment
1. **Build Docker images**
   ```bash
   # Backend
   docker build -t threat-detector-api -f src/ThreatDetector.API/Dockerfile .
   
   # Frontend
   cd client
   docker build -t threat-detector-client .
   ```

2. **Run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

### AWS Deployment
1. **Set up AWS infrastructure**
   - RDS instance for database
   - ECS or EKS for container orchestration
   - ALB for load balancing
   - CloudWatch for monitoring

2. **Deploy using AWS CLI**
   ```bash
   aws ecs update-service --cluster threat-detector --service api --force-new-deployment
   ```

## 📖 API Documentation

Once the API is running, access the interactive API documentation at:
- **Swagger UI**: `https://localhost:7001/swagger`

### Key Endpoints
- `GET /api/threatdetection/active` - Get active threats
- `POST /api/threatdetection/analyze` - Analyze data for threats
- `GET /api/userbehavior/anomalies` - Get behavior anomalies
- `POST /api/zeroday/detect` - Detect zero-day vulnerabilities
- `GET /api/alerts/active` - Get active security alerts
- `GET /api/learning/overview` - Get ML model status

## 🧪 Testing

### Backend Tests
```bash
cd src/ThreatDetector.Tests
dotnet test
```

### Frontend Tests
```bash
cd client
npm test
```

## 📈 Monitoring & Observability

### Health Checks
- API health: `https://localhost:7001/health`
- Database connectivity check
- ML model status verification

### Logging
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Critical
- Log outputs: Console, File, CloudWatch (production)

### Metrics
- Real-time threat detection rates
- Model performance metrics
- System resource utilization
- Alert response times

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation wiki

## 🔮 Future Enhancements

- **Advanced ML Models**: Integration with TensorFlow/PyTorch
- **Multi-tenant Support**: Organization-based data isolation
- **Advanced Analytics**: Predictive threat modeling
- **Mobile App**: iOS/Android companion apps
- **Integration APIs**: Third-party security tool integration
- **Compliance Reporting**: Automated security compliance reports

---

**Built with ❤️ for enhanced cybersecurity** 