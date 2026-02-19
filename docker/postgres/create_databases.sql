SELECT 'CREATE DATABASE bf_tenant_db'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'bf_tenant_db')\gexec

SELECT 'CREATE DATABASE bf_faq_db_01'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'bf_faq_db_01')\gexec

SELECT 'CREATE DATABASE bf_faq_db_02'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'bf_faq_db_02')\gexec
