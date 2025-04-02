# Real-time Air Quality Monitoring Dashboard for Colombo

## Overview
The **Real-time Air Quality Monitoring Dashboard for Colombo** is a web-based application designed to provide real-time air quality information for the Colombo Metropolitan Area. This dashboard allows users to:
- Monitor **Air Quality Index (AQI)** levels across the city.
- Explore **historical trends** in air quality.
- Understand the **spatial distribution** of air pollutants.

The system simulates data from a network of air quality sensors placed across Colombo to generate AQI readings. Users can interact with the dashboard to gain insights into air quality conditions in their environment.

## Features
- **Real-time AQI Monitoring**: View up-to-date air quality information for different locations in Colombo.
- **Historical Data Analysis**: Access past AQI trends and visualize changes over time.
- **Interactive Map Visualization**: Display air quality levels on a city-wide map.
- **Data Simulation**: The system generates AQI readings based on simulated sensor data.
- **User-Friendly Interface**: A responsive and easy-to-navigate dashboard.
- **Custom Alerts**: Set up notifications for specific AQI thresholds.
- **Multi-User Access**: Different user roles with varying access levels.
- **Mobile Compatibility**: Fully responsive design for mobile users.
- **Downloadable Reports**: Export air quality data for further analysis.

## Technology Stack
- **Frontend**: ASP.NET Core MVC, JavaScript, HTML, CSS
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **Data Processing**: .NET Core
- **Mapping & Visualization**: Leaflet.js / Google Maps API
- **Deployment**: Azure / AWS (Optional)

## Installation & Setup
### Prerequisites
Ensure you have the following installed on your system:
- .NET 6.0 or later
- SQL Server
- Visual Studio or VS Code
- Node.js (for frontend package management)

### Steps to Run the Application
1. **Clone the Repository**
   ```sh
   git clone https://github.com/your-repository/air-quality-dashboard.git
   cd air-quality-dashboard
   ```

2. **Setup Database**
   - Create a new SQL Server database.
   - Run the provided `database.sql` script to create necessary tables.

3. **Configure the Application**
   - Update the `appsettings.json` file with your database connection string.

4. **Run the Backend API**
   ```sh
   cd backend
   dotnet run
   ```

5. **Run the Frontend**
   ```sh
   cd frontend
   npm install
   npm start
   ```

6. Open your browser and navigate to `http://localhost:5000` (or the configured port).

## API Endpoints
| Method | Endpoint                 | Description                         |
|--------|--------------------------|-------------------------------------|
| GET    | `/api/aqi/latest`         | Get the latest AQI data            |
| GET    | `/api/aqi/history`        | Fetch historical air quality data  |
| GET    | `/api/locations`          | Retrieve list of monitored locations |
| POST   | `/api/alerts`             | Set up AQI threshold alerts        |
| GET    | `/api/reports`            | Generate and download AQI reports  |

## Deployment Guide
### Hosting on Azure
1. Create an **App Service** in Azure.
2. Configure the **SQL Server Database**.
3. Deploy the **API and Frontend** via Azure DevOps or GitHub Actions.

### Hosting on AWS
1. Use **AWS Elastic Beanstalk** for the .NET API.
2. Set up an **RDS SQL Server Instance**.
3. Deploy the frontend to **S3 with CloudFront** for distribution.

## Security Considerations
- Implement **JWT-based authentication** for secure API access.
- Use **HTTPS** to encrypt data transmission.
- Regularly update dependencies to mitigate security vulnerabilities.
- Enable **role-based access control (RBAC)** for different user levels.

## Contribution
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch (`feature-branch`).
3. Commit changes and push to your branch.
4. Create a Pull Request.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact
For inquiries or support, reach out to **your.email@example.com** or open an issue on GitHub.

## Future Enhancements
- **AI-Powered Predictions**: Forecast air quality trends using machine learning.
- **IoT Integration**: Connect with physical air quality sensors for real data.
- **Multi-City Expansion**: Extend support for other regions.
- **Social Sharing**: Share air quality reports via social media platforms.
- **User Feedback Mechanism**: Collect user input to improve the dashboard.