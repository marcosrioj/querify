SELECT 'CREATE DATABASE qf_tenant_db'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'qf_tenant_db')\gexec

SELECT 'CREATE DATABASE qf_qna_db_01'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'qf_qna_db_01')\gexec

SELECT 'CREATE DATABASE qf_qna_db_02'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'qf_qna_db_02')\gexec

SELECT 'CREATE DATABASE qf_hangfire_db'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'qf_hangfire_db')\gexec
