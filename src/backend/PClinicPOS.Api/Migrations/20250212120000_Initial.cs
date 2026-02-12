using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PClinicPOS.Api.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder m)
        {
            m.Sql(@"
                CREATE TABLE ""Tenants"" (
                    ""Id"" uuid NOT NULL,
                    ""Name"" character varying(200) NOT NULL,
                    CONSTRAINT ""PK_Tenants"" PRIMARY KEY (""Id"")
                );

                CREATE TABLE ""Branches"" (
                    ""Id"" uuid NOT NULL,
                    ""TenantId"" uuid NOT NULL,
                    ""Name"" character varying(200) NOT NULL,
                    CONSTRAINT ""PK_Branches"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_Branches_Tenants"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX ""IX_Branches_TenantId"" ON ""Branches"" (""TenantId"");

                CREATE TABLE ""Patients"" (
                    ""Id"" uuid NOT NULL,
                    ""FirstName"" character varying(200) NOT NULL,
                    ""LastName"" character varying(200) NOT NULL,
                    ""PhoneNumber"" character varying(50) NOT NULL,
                    ""TenantId"" uuid NOT NULL,
                    ""PrimaryBranchId"" uuid NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_Patients"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_Patients_Tenants"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Patients_Branches"" FOREIGN KEY (""PrimaryBranchId"") REFERENCES ""Branches"" (""Id"") ON DELETE SET NULL
                );
                CREATE UNIQUE INDEX ""IX_Patients_TenantId_PhoneNumber"" ON ""Patients"" (""TenantId"", ""PhoneNumber"");
                CREATE INDEX ""IX_Patients_TenantId"" ON ""Patients"" (""TenantId"");
                CREATE INDEX ""IX_Patients_PrimaryBranchId"" ON ""Patients"" (""PrimaryBranchId"");

                CREATE TABLE ""Users"" (
                    ""Id"" uuid NOT NULL,
                    ""Email"" character varying(256) NOT NULL,
                    ""PasswordHash"" character varying(500) NOT NULL,
                    ""Role"" character varying(50) NOT NULL,
                    ""TenantId"" uuid NOT NULL,
                    CONSTRAINT ""PK_Users"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_Users_Tenants"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE RESTRICT
                );
                CREATE UNIQUE INDEX ""IX_Users_TenantId_Email"" ON ""Users"" (""TenantId"", ""Email"");

                CREATE TABLE ""UserBranch"" (
                    ""UserId"" uuid NOT NULL,
                    ""BranchId"" uuid NOT NULL,
                    CONSTRAINT ""PK_UserBranch"" PRIMARY KEY (""UserId"", ""BranchId""),
                    CONSTRAINT ""FK_UserBranch_Users"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserBranch_Branches"" FOREIGN KEY (""BranchId"") REFERENCES ""Branches"" (""Id"") ON DELETE CASCADE
                );

                CREATE TABLE ""Appointments"" (
                    ""Id"" uuid NOT NULL,
                    ""TenantId"" uuid NOT NULL,
                    ""BranchId"" uuid NOT NULL,
                    ""PatientId"" uuid NOT NULL,
                    ""StartAt"" timestamp with time zone NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_Appointments"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_Appointments_Tenants"" FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Appointments_Branches"" FOREIGN KEY (""BranchId"") REFERENCES ""Branches"" (""Id"") ON DELETE RESTRICT,
                    CONSTRAINT ""FK_Appointments_Patients"" FOREIGN KEY (""PatientId"") REFERENCES ""Patients"" (""Id"") ON DELETE RESTRICT
                );
                CREATE UNIQUE INDEX ""IX_Appointments_TenantId_BranchId_PatientId_StartAt"" ON ""Appointments"" (""TenantId"", ""BranchId"", ""PatientId"", ""StartAt"");
                CREATE INDEX ""IX_Appointments_TenantId"" ON ""Appointments"" (""TenantId"");
            ");
        }

        protected override void Down(MigrationBuilder m)
        {
            m.Sql(@"
                DROP TABLE IF EXISTS ""Appointments"";
                DROP TABLE IF EXISTS ""UserBranch"";
                DROP TABLE IF EXISTS ""Users"";
                DROP TABLE IF EXISTS ""Patients"";
                DROP TABLE IF EXISTS ""Branches"";
                DROP TABLE IF EXISTS ""Tenants"";
            ");
        }
    }
}
