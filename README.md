# End-to-End Data Pipeline with Batch & Streaming Processing

This repository contains a **fully integrated, production-ready data pipeline** that supports both **batch** and **streaming** data processing using open-source technologies. It is designed to be easily configured and deployed by any business or individual with minimal modifications.

The pipeline incorporates:

- **Data Ingestion:**  
  - **Batch Sources:** SQL databases (MySQL, PostgreSQL), Data Lakes (MinIO as an S3-compatible store), files (CSV, JSON, XML)  
  - **Streaming Sources:** Kafka for event logs, IoT sensor data, and social media streams

- **Data Processing & Transformation:**  
  - **Batch Processing:** Apache Spark for large-scale ETL jobs, integrated with Great Expectations for data quality checks  
  - **Streaming Processing:** Spark Structured Streaming for real-time data processing and anomaly detection

- **Data Storage:**  
  - **Raw Data:** Stored in MinIO (S3-compatible storage)  
  - **Processed Data:** Loaded into PostgreSQL for analytics and reporting

- **Data Quality, Monitoring & Governance:**  
  - **Data Quality:** Great Expectations validates incoming data  
  - **Data Governance:** Apache Atlas / OpenMetadata integration (lineage registration)  
  - **Monitoring & Logging:** Prometheus and Grafana for system monitoring and alerting

- **Data Serving & AI/ML Integration:**  
  - **ML Pipelines:** MLflow for model tracking and feature store integration  
  - **BI & Dashboarding:** Grafana dashboards provide real-time insights

- **CI/CD & Deployment:**  
  - **CI/CD Pipelines:** GitHub Actions or Jenkins for continuous integration and deployment  
  - **Container Orchestration:** Kubernetes with Argo CD for GitOps deployment

[![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/) [![SQL](https://img.shields.io/badge/SQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/) [![Bash](https://img.shields.io/badge/Bash-4EAA25?style=for-the-badge&logo=gnubash&logoColor=white)](https://www.gnu.org/software/bash/) [![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/) [![Kubernetes](https://img.shields.io/badge/Kubernetes-326CE5?style=for-the-badge&logo=kubernetes&logoColor=white)](https://kubernetes.io/) [![Apache Airflow](https://img.shields.io/badge/Apache%20Airflow-017CEE?style=for-the-badge&logo=apacheairflow&logoColor=white)](https://airflow.apache.org/) [![Apache Spark](https://img.shields.io/badge/Apache%20Spark-E25A1C?style=for-the-badge&logo=apachespark&logoColor=white)](https://spark.apache.org/) [![Apache Flink](https://img.shields.io/badge/Apache%20Flink-E6526F?style=for-the-badge&logo=apacheflink&logoColor=white)](https://flink.apache.org/) [![Kafka](https://img.shields.io/badge/Apache%20Kafka-231F20?style=for-the-badge&logo=apachekafka&logoColor=white)](https://kafka.apache.org/) [![Apache Hadoop](https://img.shields.io/badge/Apache%20Hadoop-EC1D24?style=for-the-badge&logo=apachehadoop&logoColor=white)](https://hadoop.apache.org/)
 [![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/) [![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/) [![MongoDB](https://img.shields.io/badge/MongoDB-47A248?style=for-the-badge&logo=mongodb&logoColor=white)](https://www.mongodb.com/) [![InfluxDB](https://img.shields.io/badge/InfluxDB-22ADF6?style=for-the-badge&logo=influxdb&logoColor=white)](https://www.influxdata.com/) [![MinIO](https://img.shields.io/badge/MinIO-CF2A27?style=for-the-badge&logo=minio&logoColor=white)](https://min.io/) [![AWS S3](https://img.shields.io/badge/AWS%20S3-232F3E?style=for-the-badge&logo=amazonaws&logoColor=white)](https://aws.amazon.com/s3/) [![Prometheus](https://img.shields.io/badge/Prometheus-E6522C?style=for-the-badge&logo=prometheus&logoColor=white)](https://prometheus.io/) [![Grafana](https://img.shields.io/badge/Grafana-F46800?style=for-the-badge&logo=grafana&logoColor=white)](https://grafana.com/) [![Elasticsearch](https://img.shields.io/badge/Elasticsearch-005571?style=for-the-badge&logo=elasticsearch&logoColor=white)](https://www.elastic.co/) [![MLflow](https://img.shields.io/badge/MLflow-0194E2?style=for-the-badge&logo=mlflow&logoColor=white)](https://mlflow.org/) [![Feast](https://img.shields.io/badge/Feast-FF6F00?style=for-the-badge&logo=feast&logoColor=white)](https://feast.dev/) [![Great Expectations](https://img.shields.io/badge/Great%20Expectations-1A1A1A?style=for-the-badge&logo=great-expectations&logoColor=white)](https://greatexpectations.io/) [![Apache Atlas](https://img.shields.io/badge/Apache%20Atlas-1E1E1E?style=for-the-badge&logo=apache&logoColor=white)](https://atlas.apache.org/) [![Tableau](https://img.shields.io/badge/Tableau-E97627?style=for-the-badge&logo=tableau&logoColor=white)](https://www.tableau.com/) [![Power BI](https://img.shields.io/badge/Power%20BI-F2C811?style=for-the-badge&logo=powerbi&logoColor=white)](https://powerbi.microsoft.com/) [![Looker](https://img.shields.io/badge/Looker-4285F4?style=for-the-badge&logo=looker&logoColor=white)](https://looker.com/) [![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/) [![Terraform](https://img.shields.io/badge/Terraform-7B42BC?style=for-the-badge&logo=terraform&logoColor=white)](https://www.terraform.io/)

Read this README and follow the step-by-step guide to set up the pipeline on your local machine or cloud environment. Customize the pipeline components, configurations, and example applications to suit your data processing needs.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Directory Structure](#directory-structure)
3. [Components & Technologies](#components--technologies)
4. [Setup Instructions](#setup-instructions)
5. [Configuration & Customization](#configuration--customization)
6. [Example Applications](#example-applications)
7. [Troubleshooting & Further Considerations](#troubleshooting--further-considerations)
8. [Contributing](#contributing)
9. [License](#license)
10. [Final Notes](#final-notes)

## Architecture Overview

The architecture of the end-to-end data pipeline is designed to handle both batch and streaming data processing. Below is a high-level overview of the components and their interactions:

### High-Level Architecture

```mermaid
graph TB
    subgraph "Data Sources"
        BS[Batch Sources<br/>MySQL, Files, CSV/JSON/XML]
        SS[Streaming Sources<br/>Kafka Events, IoT, Social Media]
    end

    subgraph "Ingestion & Orchestration"
        AIR[Apache Airflow<br/>DAG Orchestration]
        KAF[Apache Kafka<br/>Event Streaming]
    end

    subgraph "Processing Layer"
        SPB[Spark Batch<br/>Large-scale ETL]
        SPS[Spark Streaming<br/>Real-time Processing]
        GE[Great Expectations<br/>Data Quality]
    end

    subgraph "Storage Layer"
        MIN[MinIO<br/>S3-Compatible Storage]
        PG[PostgreSQL<br/>Analytics Database]
        S3[AWS S3<br/>Cloud Storage]
        MDB[MongoDB<br/>NoSQL Store]
        IDB[InfluxDB<br/>Time-series DB]
    end

    subgraph "Monitoring & Governance"
        PROM[Prometheus<br/>Metrics Collection]
        GRAF[Grafana<br/>Dashboards]
        ATL[Apache Atlas<br/>Data Lineage]
    end

    subgraph "ML & Serving"
        MLF[MLflow<br/>Model Tracking]
        FST[Feast<br/>Feature Store]
        BI[BI Tools<br/>Tableau/PowerBI/Looker]
    end

    BS --> AIR
    SS --> KAF
    AIR --> SPB
    KAF --> SPS
    SPB --> GE
    SPS --> GE
    GE --> MIN
    GE --> PG
    MIN --> S3
    PG --> MDB
    PG --> IDB
    SPB --> PROM
    SPS --> PROM
    PROM --> GRAF
    SPB --> ATL
    SPS --> ATL
    PG --> MLF
    PG --> FST
    PG --> BI
    MIN --> MLF
```

### Flow Diagram

<p align="center">
  <img src="assets/architecture_diagram.png" alt="Architecture Diagram" width="100%"/>
</p>

Basically, data will be streamed with Kafka, processed with Spark, and stored in a data warehouse using PostgreSQL. The pipeline also integrates MinIO as an object storage solution and uses Airflow to orchestrate the end-to-end data flow. Great Expectations enforces data quality checks, while Prometheus and Grafana provide monitoring and alerting capabilities. MLflow and Feast are used for machine learning model tracking and feature store integration.

> [!CAUTION]
> Note: The diagram(s) may not reflect ALL components in the repository, but it provides a good overview of the main components and their interactions. For instance, I added BI tools like Tableau, Power BI, and Looker to the repo for data visualization and reporting.

### Batch Pipeline Flow

```mermaid
sequenceDiagram
    participant BS as Batch Source<br/>(MySQL/Files)
    participant AF as Airflow DAG
    participant GE as Great Expectations
    participant MN as MinIO
    participant SP as Spark Batch
    participant PG as PostgreSQL
    participant MG as MongoDB
    participant PR as Prometheus

    BS->>AF: Trigger Batch Job
    AF->>BS: Extract Data
    AF->>GE: Validate Data Quality
    GE-->>AF: Validation Results
    AF->>MN: Upload Raw Data
    AF->>SP: Submit Spark Job
    SP->>MN: Read Raw Data
    SP->>SP: Transform & Enrich
    SP->>PG: Write Processed Data
    SP->>MG: Write NoSQL Data
    SP->>PR: Send Metrics
    AF->>PR: Job Status Metrics
```

### Streaming Pipeline Flow

```mermaid
sequenceDiagram
    participant KP as Kafka Producer
    participant KT as Kafka Topic
    participant SS as Spark Streaming
    participant AD as Anomaly Detection
    participant PG as PostgreSQL
    participant MN as MinIO
    participant GF as Grafana

    KP->>KT: Publish Events
    KT->>SS: Consume Stream
    SS->>AD: Process Events
    AD->>AD: Detect Anomalies
    AD->>PG: Store Results
    AD->>MN: Archive Data
    SS->>GF: Real-time Metrics
    GF->>GF: Update Dashboard
```

### Data Quality & Governance Flow

```mermaid
graph LR
    subgraph "Data Quality Pipeline"
        DI[Data Ingestion] --> GE[Great Expectations]
        GE --> VR{Validation<br/>Result}
        VR -->|Pass| DP[Data Processing]
        VR -->|Fail| AL[Alert & Log]
        AL --> DR[Data Rejection]
        DP --> DQ[Quality Metrics]
    end

    subgraph "Data Governance"
        DP --> ATL[Apache Atlas]
        ATL --> LIN[Lineage Tracking]
        ATL --> CAT[Data Catalog]
        ATL --> POL[Policies & Compliance]
    end

    DQ --> PROM[Prometheus]
    PROM --> GRAF[Grafana Dashboard]
```

### CI/CD & Deployment Pipeline

```mermaid
graph LR
    subgraph "Development"
        DEV[Developer] --> GIT[Git Push]
    end

    subgraph "CI/CD Pipeline"
        GIT --> GHA[GitHub Actions]
        GHA --> TEST[Run Tests]
        TEST --> BUILD[Build Docker Images]
        BUILD --> SCAN[Security Scan]
        SCAN --> PUSH[Push to Registry]
    end

    subgraph "Deployment"
        PUSH --> ARGO[Argo CD]
        ARGO --> K8S[Kubernetes Cluster]
        K8S --> HELM[Helm Charts]
        HELM --> PODS[Deploy Pods]
    end

    subgraph "Infrastructure"
        TERRA[Terraform] --> CLOUD[Cloud Resources]
        CLOUD --> K8S
    end

    PODS --> MON[Monitoring]
```

### Text-Based Pipeline Diagram

```mermaid
graph TB
    subgraph Batch["Batch Processing"]
        BS[Batch Source<br/>MySQL, Files, User Interaction]
        AD[Airflow Batch DAG<br/>- Extracts data from MySQL<br/>- Validates with Great Expectations<br/>- Uploads raw data to MinIO]
        SB[Spark Batch Job<br/>- Reads raw CSV from MinIO<br/>- Transforms, cleans, enriches<br/>- Writes to PostgreSQL & MinIO]
        PDS[Processed Data Store<br/>PostgreSQL, MongoDB, AWS S3]
        CI[Cache & Indexing<br/>Elasticsearch, Redis]
        
        BS -->|Extract/Validate| AD
        AD -->|spark-submit| SB
        SB -->|Load/Analyze| PDS
        PDS -->|Query/Analyze| CI
    end
    
    subgraph Stream["Streaming Processing"]
        SS[Streaming Source<br/>Kafka]
        SSJ[Spark Streaming Job<br/>- Consumes Kafka messages<br/>- Filters and detects anomalies<br/>- Persists to PostgreSQL & MinIO]
        
        SS --> SSJ
    end
    
    subgraph Monitor["Monitoring & Governance"]
        MG[Monitoring & Data Governance<br/>- Prometheus & Grafana<br/>- Apache Atlas / OpenMetadata]
    end
    
    subgraph ML["AI/ML Serving"]
        MLS[AI/ML Serving<br/>- Feature Store Feast<br/>- MLflow Model Tracking<br/>- Model training & serving<br/>- BI Dashboards]
    end
    
    subgraph CICD["CI/CD & Infrastructure"]
        CIP[CI/CD Pipelines<br/>GitHub Actions / Jenkins<br/>Terraform for Cloud Deploy]
        K8S[Kubernetes Cluster<br/>Argo CD for GitOps<br/>Helm Charts for Deployment]
    end
    
    SSJ --> PDS
    MG -.monitors.-> Batch
    MG -.monitors.-> Stream
    ML -.uses.-> PDS
    CIP --> K8S
    K8S -.orchestrates.-> Batch
    K8S -.orchestrates.-> Stream
```

### Full Flow Diagram with Backend & Frontend Integration (Optional)

A more detailed flow diagram that includes backend and frontend integration is available in the `assets/` directory. This diagram illustrates how the data pipeline components interact with each other and with external systems, including data sources, storage, processing, visualization, and monitoring. 

Although the frontend & backend integration is not included in this repository (since it's supposed to only contain the pipeline), you can easily integrate it with your existing frontend application or create a new one using popular frameworks like React, Angular, or Vue.js.

<p align="center">
  <img src="assets/full_flow_diagram.png" alt="Full Flow Diagram" width="100%"/>
</p>

### Docker Services Architecture

```mermaid
graph TB
    subgraph "Docker Compose Stack"
        subgraph "Data Sources"
            MYSQL[MySQL<br/>Port: 3306]
            KAFKA[Kafka<br/>Port: 9092]
            ZK[Zookeeper<br/>Port: 2181]
        end

        subgraph "Processing"
            AIR[Airflow<br/>Webserver:8080<br/>Scheduler]
            SPARK[Spark<br/>Master/Worker]
        end

        subgraph "Storage"
            MINIO[MinIO<br/>API: 9000<br/>Console: 9001]
            PG[PostgreSQL<br/>Port: 5432]
        end

        subgraph "Monitoring"
            PROM[Prometheus<br/>Port: 9090]
            GRAF[Grafana<br/>Port: 3000]
        end

        KAFKA --> ZK
        AIR --> MYSQL
        AIR --> PG
        AIR --> SPARK
        SPARK --> MINIO
        SPARK --> PG
        SPARK --> KAFKA
        PROM --> AIR
        PROM --> SPARK
        GRAF --> PROM
    end
```

### ML Pipeline Flow

```mermaid
flowchart LR
    subgraph "Feature Engineering"
        RAW[Raw Data] --> FE[Feature<br/>Extraction]
        FE --> FS[Feature Store<br/>Feast]
    end

    subgraph "Model Training"
        FS --> TRAIN[Training<br/>Pipeline]
        TRAIN --> VAL[Validation]
        VAL --> MLF[MLflow<br/>Registry]
    end

    subgraph "Model Serving"
        MLF --> DEPLOY[Model<br/>Deployment]
        DEPLOY --> API[Prediction<br/>API]
        API --> APP[Applications]
    end

    subgraph "Monitoring"
        API --> METRICS[Performance<br/>Metrics]
        METRICS --> DRIFT[Drift<br/>Detection]
        DRIFT --> RETRAIN[Retrigger<br/>Training]
    end

    RETRAIN --> TRAIN
```

## Directory Structure

```
end-to-end-pipeline/
  ├── .devcontainer/                 # VS Code Dev Container settings
  ├── docker-compose.yaml            # Docker orchestration for all services
  ├── docker-compose.ci.yaml         # Docker Compose for CI/CD pipelines
  ├── End_to_End_Data_Pipeline.ipynb # Jupyter notebook for pipeline overview
  ├── requirements.txt               # Python dependencies for scripts
  ├── .gitignore                     # Standard Git ignore file
  ├── README.md                      # Comprehensive documentation (this file)
  ├── airflow/
  │   ├── Dockerfile                 # Custom Airflow image with dependencies
  │   ├── requirements.txt           # Python dependencies for Airflow
  │   └── dags/
  │       ├── batch_ingestion_dag.py # Batch pipeline DAG
  │       └── streaming_monitoring_dag.py  # Streaming monitoring DAG
  ├── spark/
  │   ├── Dockerfile                 # Custom Spark image with Kafka and S3 support
  │   ├── spark_batch_job.py         # Spark batch ETL job
  │   └── spark_streaming_job.py     # Spark streaming job
  ├── kafka/
  │   └── producer.py                # Kafka producer for simulating event streams
  ├── storage/
  │   ├── aws_s3_influxdb.py         # S3-InfluxDB integration stub
  │   ├── hadoop_batch_processing.py  # Hadoop batch processing stub
  │   └── mongodb_streaming.py       # MongoDB streaming integration stub
  ├── great_expectations/
  │   ├── great_expectations.yaml    # GE configuration
  │   └── expectations/
  │       └── raw_data_validation.py # GE suite for data quality
  ├── governance/
  │   └── atlas_stub.py              # Dataset lineage registration with Atlas/OpenMetadata
  ├── monitoring/
  │   ├── monitoring.py              # Python script to set up Prometheus & Grafana
  │   └── prometheus.yml             # Prometheus configuration file
  ├── ml/
  │   ├── feature_store_stub.py      # Feature Store integration stub
  │   └── mlflow_tracking.py         # MLflow model tracking
  ├── kubernetes/
  │   ├── argo-app.yaml              # Argo CD application manifest
  │   └── deployment.yaml            # Kubernetes deployment manifest
  ├── terraform/                     # Terraform scripts for cloud deployment
  └── scripts/
      └── init_db.sql                # SQL script to initialize MySQL and demo data
```

## Components & Technologies

- **Ingestion & Orchestration:**  
  - [Apache Airflow](https://airflow.apache.org/) – Schedules batch and streaming jobs.
  - [Kafka](https://kafka.apache.org/) – Ingests streaming events.
  - [Spark](https://spark.apache.org/) – Processes batch and streaming data.

- **Storage & Processing:**  
  - [MinIO](https://min.io/) – S3-compatible data lake.
  - [PostgreSQL](https://www.postgresql.org/) – Stores transformed and processed data.
  - [Great Expectations](https://greatexpectations.io/) – Enforces data quality.
  - [AWS S3](https://aws.amazon.com/s3/) – Cloud storage integration.
  - [InfluxDB](https://www.influxdata.com/) – Time-series data storage.
  - [MongoDB](https://www.mongodb.com/) – NoSQL database integration.
  - [Hadoop](https://hadoop.apache.org/) – Big data processing integration.

- **Monitoring & Governance:**  
  - [Prometheus](https://prometheus.io/) – Metrics collection.
  - [Grafana](https://grafana.com/) – Dashboard visualization.
  - [Apache Atlas/OpenMetadata](https://atlas.apache.org/) – Data lineage and governance.

- **ML & Data Serving:**  
  - [MLflow](https://mlflow.org/) – Experiment tracking.
  - [Feast](https://feast.dev/) – Feature store for machine learning.
  - [BI Tools](https://grafana.com/) – Real-time dashboards and insights.

## Setup Instructions

### Prerequisites

- **Docker** and **Docker Compose** must be installed.
- Ensure that **Python 3.9+** is installed locally if you want to run scripts outside of Docker.
- Open ports required:  
  - Airflow: 8080  
  - MySQL: 3306  
  - PostgreSQL: 5432  
  - MinIO: 9000 (and console on 9001)  
  - Kafka: 9092  
  - Prometheus: 9090  
  - Grafana: 3000  

### Step-by-Step Guide

1. **Clone the Repository**

   ```bash
   git clone https://github.com/hoangsonww/End-to-End-Data-Pipeline.git
   cd End-to-End-Data-Pipeline
   ```

2. **Start the Pipeline Stack**

   Use Docker Compose to launch all components:
   
   ```bash
   docker-compose up --build
   ```
   
   This command will:
   - Build custom Docker images for Airflow and Spark.
   - Start MySQL, PostgreSQL, Kafka (with Zookeeper), MinIO, Prometheus, Grafana, and Airflow webserver.
   - Initialize the MySQL database with demo data (via `scripts/init_db.sql`).

3. **Access the Services**
   - **Airflow UI:** [http://localhost:8080](http://localhost:8080)  
     Set up connections:  
     - `mysql_default` → Host: `mysql`, DB: `source_db`, User: `user`, Password: `pass`
     - `postgres_default` → Host: `postgres`, DB: `processed_db`, User: `user`, Password: `pass`
   - **MinIO Console:** [http://localhost:9001](http://localhost:9001) (User: `minio`, Password: `minio123`)
   - **Kafka:** Accessible on port `9092`
   - **Prometheus:** [http://localhost:9090](http://localhost:9090)
   - **Grafana:** [http://localhost:3000](http://localhost:3000) (Default login: `admin/admin`)

4. **Run Batch Pipeline**
   - In the Airflow UI, enable the `batch_ingestion_dag` to run the end-to-end batch pipeline.
   - This DAG extracts data from MySQL, validates it, uploads raw data to MinIO, triggers a Spark job for transformation, and loads data into PostgreSQL.

5. **Run Streaming Pipeline**
   - Open a terminal and start the Kafka producer:
     ```bash
     docker-compose exec kafka python /opt/spark_jobs/../kafka/producer.py
     ```
   - In another terminal, run the Spark streaming job:
     ```bash
     docker-compose exec spark spark-submit --master local[2] /opt/spark_jobs/spark_streaming_job.py
     ```
   - The streaming job consumes events from Kafka, performs real-time anomaly detection, and writes results to PostgreSQL and MinIO.

6. **Monitoring & Governance**
   - **Prometheus & Grafana:**  
     Use the `monitoring.py` script (or access Grafana) to view real-time metrics and dashboards.
   - **Data Lineage:**  
     The `governance/atlas_stub.py` script registers lineage between datasets (can be extended for full Apache Atlas integration).

7. **ML & Feature Store**
   - Use `ml/mlflow_tracking.py` to simulate model training and tracking.
   - Use `ml/feature_store_stub.py` to integrate with a feature store like Feast.

8. **CI/CD & Deployment**
    - Use the `docker-compose.ci.yaml` file to set up CI/CD pipelines.
    - Use the `kubernetes/` directory for Kubernetes deployment manifests.
    - Use the `terraform/` directory for cloud deployment scripts.
    - Use the `.github/workflows/` directory for GitHub Actions CI/CD workflows.

### Next Steps

Congratulations! You have successfully set up the end-to-end data pipeline with batch and streaming processing. However, this is a very general pipeline that needs to be customized for your specific use case.

> [!IMPORTANT]
> Note: Be sure to visit the files and scripts in the repository and change the credentials, configurations, and logic to match your environment and use case. Feel free to extend the pipeline with additional components, services, or integrations as needed.

## Configuration & Customization

- **Docker Compose:**  
  All services are defined in `docker-compose.yaml`. Adjust resource limits, environment variables, and service dependencies as needed.

- **Airflow:**  
  Customize DAGs in the `airflow/dags/` directory. Use the provided PythonOperators to integrate custom processing logic.

- **Spark Jobs:**  
  Edit transformation logic in `spark/spark_batch_job.py` and `spark/spark_streaming_job.py` to match your data and processing requirements.

- **Kafka Producer:**  
  Modify `kafka/producer.py` to simulate different types of events or adjust the batch size and frequency using environment variables.

- **Monitoring:**  
  Update `monitoring/monitoring.py` and `prometheus.yml` to scrape additional metrics or customize dashboards. Place Grafana dashboard JSON files in the `monitoring/grafana_dashboards/` directory.

- **Governance & ML:**  
  Replace stub implementations in `governance/atlas_stub.py` and `ml/` with real integrations as needed.

- **CI/CD & Deployment:**  
  Customize CI/CD workflows in `.github/workflows/` and deployment manifests in `kubernetes/` and `terraform/` for your cloud environment.

- **Storage:**

    Data storage options are in the `storage/` directory with AWS S3, InfluxDB, MongoDB, and Hadoop stubs. Replace these with real integrations or credentials as needed.

## Example Applications

```mermaid
%%{init: {"theme": "default", "themeVariables": { "primaryColor": "#f9f9f9", "fontSize": "14px", "lineColor": "#000000", "textColor": "#000000", "background": "#ffffff"}}}%%
mindmap
  root((Data Pipeline<br/>Use Cases))
    E-Commerce
      Real-Time Recommendations
        Clickstream Processing
        User Behavior Analysis
        Personalized Content
      Fraud Detection
        Transaction Monitoring
        Pattern Recognition
        Risk Scoring
    Finance
      Risk Analysis
        Credit Assessment
        Portfolio Analytics
        Market Risk
      Trade Surveillance
        Market Data Processing
        Compliance Monitoring
        Anomaly Detection
    Healthcare
      Patient Monitoring
        IoT Sensor Data
        Real-time Alerts
        Predictive Analytics
      Clinical Trials
        Data Integration
        Outcome Prediction
        Drug Efficacy Analysis
    IoT/Manufacturing
      Predictive Maintenance
        Sensor Analytics
        Failure Prediction
        Maintenance Scheduling
      Supply Chain
        Inventory Optimization
        Logistics Tracking
        Demand Forecasting
    Media
      Sentiment Analysis
        Social Media Streams
        Brand Monitoring
        Trend Detection
      Ad Fraud Detection
        Click Pattern Analysis
        Bot Detection
        Campaign Analytics
```

### E-Commerce & Retail
- **Real-Time Recommendations:**
  Process clickstream data to generate personalized product recommendations.
- **Fraud Detection:**
  Detect unusual purchasing patterns or multiple high-value transactions in real-time.

### Financial Services & Banking
- **Risk Analysis:**
  Aggregate transaction data to assess customer credit risk.
- **Trade Surveillance:**
  Monitor market data and employee trades for insider trading signals.

### Healthcare & Life Sciences
- **Patient Monitoring:**
  Process sensor data from medical devices to alert healthcare providers of critical conditions.
- **Clinical Trial Analysis:**
  Analyze historical trial data for predictive analytics in treatment outcomes.

### IoT & Manufacturing
- **Predictive Maintenance:**
  Monitor sensor data from machinery to predict failures before they occur.
- **Supply Chain Optimization:**
  Aggregate data across manufacturing processes to optimize production and logistics.

### Media & Social Networks
- **Sentiment Analysis:**
  Analyze social media feeds in real-time to gauge public sentiment on new releases.
- **Ad Fraud Detection:**
  Identify and block fraudulent clicks on digital advertisements.

Feel free to use this pipeline as a starting point for your data processing needs. Extend it with additional components, services, or integrations to build a robust, end-to-end data platform.

## Troubleshooting & Further Considerations

- **Service Not Starting:**  
  Check Docker logs (`docker-compose logs`) to troubleshoot errors with MySQL, Kafka, Airflow, or Spark.
- **Airflow Connection Issues:**  
  Verify that connection settings (host, user, password) in the Airflow UI match those in `docker-compose.yaml`.
- **Data Quality Errors:**  
  Inspect Great Expectations logs in the Airflow DAG runs to adjust expectations and clean data.
- **Resource Constraints:**  
  For production use, consider scaling out services (e.g., running Spark on a dedicated cluster, using managed Kafka).

## Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request
6. We will review your changes and merge them into the main branch upon approval.

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Final Notes

> [!NOTE]
> This end-to-end data pipeline is designed for rapid deployment and customization. With minor configuration changes, it can be adapted to many business cases—from real-time analytics and fraud detection to predictive maintenance and advanced ML model training. Enjoy building a data-driven future with this pipeline!

---

Thanks for reading! If you found this repository helpful, please star it and share it with others. For questions, feedback, or suggestions, feel free to reach out to me on [GitHub](https://github.com/hoangsonww).

[**⬆️ Back to top**](#end-to-end-data-pipeline-with-batch--streaming-processing)
