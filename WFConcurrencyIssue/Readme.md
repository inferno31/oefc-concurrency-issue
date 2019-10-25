# Setup

- Create a Oracle XE instance in a docker container using this repo: https://github.com/fuzziebrain/docker-oracle-xe

- Logon as SYS with SYSDBA permissions AND Execute the following commands:
    ```sql
    alter session set "_ORACLE_SCRIPT"=true; 

    create user WFC identified by Oracle18;

    GRANT CREATE SESSION TO WFC; 

    GRANT RESOURCE to WFC;

    ALTER USER WFC QUOTA 100M ON USERS;
    
    GRANT UNLIMITED TABLESPACE TO WFC;
    ```

- Run the sample