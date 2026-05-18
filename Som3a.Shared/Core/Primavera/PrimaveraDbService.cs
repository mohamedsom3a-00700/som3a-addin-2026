using Som3a.Shared.Models.Primavera;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Som3a.Shared.Core.Primavera
{
    /// <summary>
    /// Service for connecting to and retrieving data from Primavera P6 databases.
    /// </summary>
    public class PrimaveraDbService : IPrimaveraDbService
    {
        private const int CommandTimeout = 300;

        public async Task<bool> TestConnectionAsync(string connectionString, string databaseType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    return false;

                if (IsSQLite(databaseType))
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        return connection.State == ConnectionState.Open;
                    }
                }

                if (IsSqlServer(databaseType))
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        return connection.State == ConnectionState.Open;
                    }
                }

                if (IsOracle(databaseType))
                    return await TestOracleConnectionAsync(connectionString);

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProjectDto>> GetProjectsAsync(string connectionString, string databaseType)
        {
            try
            {
                if (IsSQLite(databaseType))
                    return await GetProjectsFromSQLiteAsync(connectionString);

                if (IsSqlServer(databaseType))
                    return await GetProjectsFromSqlServerAsync(connectionString);

                if (IsOracle(databaseType))
                    return await GetProjectsFromOracleAsync(connectionString);

                throw new NotSupportedException($"Database type '{databaseType}' is not supported.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving projects: {ex.Message}");
                throw new InvalidOperationException("Could not retrieve Primavera projects from the selected database.", ex);
            }
        }

        public async Task<List<ActivityDto>> GetActivitiesAsync(string connectionString, string databaseType, int projectId)
        {
            try
            {
                if (IsSQLite(databaseType))
                    return await GetActivitiesFromSQLiteAsync(connectionString, projectId);

                if (IsSqlServer(databaseType))
                    return await GetActivitiesFromSqlServerAsync(connectionString, projectId);

                if (IsOracle(databaseType))
                    return await GetActivitiesFromOracleAsync(connectionString, projectId);

                throw new NotSupportedException($"Database type '{databaseType}' is not supported.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving activities: {ex.Message}");
                throw new InvalidOperationException("Could not retrieve Primavera activities for the selected project.", ex);
            }
        }

        public async Task<List<RelationshipDto>> GetRelationshipsAsync(string connectionString, string databaseType, int projectId)
        {
            try
            {
                if (IsSQLite(databaseType))
                    return await GetRelationshipsFromSQLiteAsync(connectionString, projectId);

                if (IsSqlServer(databaseType))
                    return await GetRelationshipsFromSqlServerAsync(connectionString, projectId);

                if (IsOracle(databaseType))
                    return await GetRelationshipsFromOracleAsync(connectionString, projectId);

                throw new NotSupportedException($"Database type '{databaseType}' is not supported.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving relationships: {ex.Message}");
                throw new InvalidOperationException("Could not retrieve Primavera relationships for the selected project.", ex);
            }
        }

        public async Task<List<ResourceDto>> GetResourcesAsync(string connectionString, string databaseType, int projectId)
        {
            try
            {
                if (IsSQLite(databaseType))
                    return await GetResourcesFromSQLiteAsync(connectionString, projectId);

                if (IsSqlServer(databaseType))
                    return await GetResourcesFromSqlServerAsync(connectionString, projectId);

                if (IsOracle(databaseType))
                    return await GetResourcesFromOracleAsync(connectionString, projectId);

                throw new NotSupportedException($"Database type '{databaseType}' is not supported.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving resources: {ex.Message}");
                throw new InvalidOperationException("Could not retrieve Primavera resource assignments for the selected project.", ex);
            }
        }

        public async Task<ProjectDto> GetProjectByIdAsync(string connectionString, string databaseType, int projectId)
        {
            var projects = await GetProjectsAsync(connectionString, databaseType);
            return projects.FirstOrDefault(p => p.ProjectId == projectId);
        }

        private static bool IsSQLite(string databaseType)
        {
            return string.Equals(databaseType, "SQLite", StringComparison.OrdinalIgnoreCase)
                || string.Equals(databaseType, "Sqlite", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSqlServer(string databaseType)
        {
            return string.Equals(databaseType, "SqlServer", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOracle(string databaseType)
        {
            return string.Equals(databaseType, "Oracle", StringComparison.OrdinalIgnoreCase);
        }

        #region SQLite Implementation

        private async Task<List<ProjectDto>> GetProjectsFromSQLiteAsync(string connectionString)
        {
            var projects = new List<ProjectDto>();

            const string query = @"
                SELECT
                    p.proj_id AS ProjectId,
                    IFNULL(p.proj_short_name, '') AS ProjectCode,
                    IFNULL(root.wbs_name, IFNULL(p.proj_short_name, '')) AS ProjectName,
                    p.add_date AS CreatedDate,
                    IFNULL(root.status_code, '') AS Status,
                    '' AS Description,
                    '' AS ProjectManager,
                    p.plan_start_date AS PlannedStartDate,
                    p.plan_end_date AS PlannedFinishDate,
                    NULL AS ActualStartDate,
                    NULL AS ActualFinishDate,
                    CAST(0 AS REAL) AS CompletionPercentage
                FROM PROJECT p
                LEFT JOIN PROJWBS root ON root.proj_id = p.proj_id AND root.proj_node_flag = 'Y'
                ORDER BY ProjectName";

            using (var reader = await ExecuteSQLiteReaderAsync(connectionString, query))
            {
                while (await reader.ReadAsync())
                {
                    projects.Add(new ProjectDto
                    {
                        ProjectId = GetInt32(reader, 0),
                        ProjectCode = GetString(reader, 1),
                        ProjectName = GetString(reader, 2),
                        CreatedDate = GetDateTime(reader, 3) ?? DateTime.MinValue,
                        Status = GetString(reader, 4),
                        Description = GetString(reader, 5),
                        ProjectManager = GetString(reader, 6),
                        PlannedStartDate = GetDateTime(reader, 7),
                        PlannedFinishDate = GetDateTime(reader, 8),
                        ActualStartDate = GetDateTime(reader, 9),
                        ActualFinishDate = GetDateTime(reader, 10),
                        CompletionPercentage = GetDecimal(reader, 11)
                    });
                }
            }

            return projects;
        }

        private async Task<List<ActivityDto>> GetActivitiesFromSQLiteAsync(string connectionString, int projectId)
        {
            var activities = new List<ActivityDto>();

            const string query = @"
                SELECT
                    t.task_id AS TaskId,
                    IFNULL(t.task_code, '') AS TaskCode,
                    IFNULL(t.task_name, '') AS TaskName,
                    IFNULL(t.status_code, '') AS StatusCode,
                    IFNULL(t.wbs_id, 0) AS WbsId,
                    IFNULL(w.wbs_name, '') AS WbsFullName,
                    IFNULL(t.phys_complete_pct, 0) AS CompletePct,
                    t.act_start_date AS ActStartDate,
                    t.act_end_date AS ActEndDate,
                    t.target_start_date AS PlanStartDate,
                    t.target_end_date AS PlanEndDate,
                    IFNULL(t.target_drtn_hr_cnt, 0) AS Duration,
                    IFNULL(t.task_type, '') AS TaskType,
                    '' AS ResourceName,
                    IFNULL(t.remain_drtn_hr_cnt, 0) AS RemainingDuration,
                    IFNULL(t.target_drtn_hr_cnt, 0) - IFNULL(t.remain_drtn_hr_cnt, 0) AS ActualDuration,
                    IFNULL(t.free_float_hr_cnt, 0) AS FreeFloat,
                    IFNULL(t.total_float_hr_cnt, 0) AS TotalFloat,
                    t.proj_id AS ProjectId
                FROM TASK t
                LEFT JOIN PROJWBS w ON t.wbs_id = w.wbs_id
                WHERE t.proj_id = @ProjectId
                ORDER BY t.task_code";

            using (var reader = await ExecuteSQLiteReaderAsync(connectionString, query, CreateSQLiteProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    activities.Add(new ActivityDto
                    {
                        TaskId = GetInt32(reader, 0),
                        TaskCode = GetString(reader, 1),
                        TaskName = GetString(reader, 2),
                        StatusCode = GetString(reader, 3),
                        WbsId = GetInt32(reader, 4),
                        WbsFullName = GetString(reader, 5),
                        CompletePct = GetDecimal(reader, 6),
                        ActStartDate = GetDateTime(reader, 7),
                        ActEndDate = GetDateTime(reader, 8),
                        PlanStartDate = GetDateTime(reader, 9),
                        PlanEndDate = GetDateTime(reader, 10),
                        Duration = HoursToDays(GetDecimal(reader, 11)),
                        TaskType = GetString(reader, 12),
                        ResourceName = GetString(reader, 13),
                        RemainingDuration = HoursToDays(GetDecimal(reader, 14)),
                        ActualDuration = HoursToDays(GetDecimal(reader, 15)),
                        FreeFloat = HoursToDays(GetDecimal(reader, 16)),
                        TotalFloat = HoursToDays(GetDecimal(reader, 17)),
                        ProjectId = GetInt32(reader, 18)
                    });
                }
            }

            return activities;
        }

        private async Task<List<RelationshipDto>> GetRelationshipsFromSQLiteAsync(string connectionString, int projectId)
        {
            var relationships = new List<RelationshipDto>();

            const string query = @"
                SELECT
                    tp.task_pred_id AS TaskPredId,
                    tp.pred_task_id AS PredTaskId,
                    tp.task_id AS TaskId,
                    IFNULL(tp.pred_type, '') AS PredType,
                    IFNULL(pred.status_code, '') AS PredTaskStatusCode,
                    IFNULL(succ.status_code, '') AS TaskStatusCode,
                    IFNULL(predwbs.wbs_name, '') AS PredTaskWbsFullName,
                    IFNULL(succwbs.wbs_name, '') AS TaskWbsFullName,
                    IFNULL(tp.lag_hr_cnt, 0) AS LagHrCnt,
                    IFNULL(pred.task_name, '') AS PredTaskName,
                    IFNULL(succ.task_name, '') AS TaskName,
                    IFNULL(pred.task_code, '') AS PredTaskCode,
                    IFNULL(succ.task_code, '') AS TaskCode,
                    succ.proj_id AS ProjectId,
                    'Hours' AS LagType
                FROM TASKPRED tp
                INNER JOIN TASK succ ON tp.task_id = succ.task_id
                LEFT JOIN TASK pred ON tp.pred_task_id = pred.task_id
                LEFT JOIN PROJWBS predwbs ON pred.wbs_id = predwbs.wbs_id
                LEFT JOIN PROJWBS succwbs ON succ.wbs_id = succwbs.wbs_id
                WHERE succ.proj_id = @ProjectId
                ORDER BY pred.task_code, succ.task_code";

            using (var reader = await ExecuteSQLiteReaderAsync(connectionString, query, CreateSQLiteProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    relationships.Add(new RelationshipDto
                    {
                        TaskPredId = GetInt32(reader, 0),
                        PredTaskId = GetInt32(reader, 1),
                        TaskId = GetInt32(reader, 2),
                        PredType = GetString(reader, 3),
                        PredTaskStatusCode = GetString(reader, 4),
                        TaskStatusCode = GetString(reader, 5),
                        PredTaskWbsFullName = GetString(reader, 6),
                        TaskWbsFullName = GetString(reader, 7),
                        LagHrCnt = GetDecimal(reader, 8),
                        PredTaskName = GetString(reader, 9),
                        TaskName = GetString(reader, 10),
                        PredTaskCode = GetString(reader, 11),
                        TaskCode = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        LagType = GetString(reader, 14)
                    });
                }
            }

            return relationships;
        }

        private async Task<List<ResourceDto>> GetResourcesFromSQLiteAsync(string connectionString, int projectId)
        {
            var resources = new List<ResourceDto>();

            const string query = @"
                SELECT
                    tr.taskrsrc_id AS RsrcId,
                    tr.task_id AS TaskId,
                    IFNULL(t.status_code, '') AS TaskStatusCode,
                    IFNULL(r.rsrc_name, '') AS RsrcName,
                    IFNULL(t.task_name, '') AS TaskName,
                    IFNULL(t.task_code, '') AS TaskCode,
                    IFNULL(r.rsrc_type, '') AS RsrcType,
                    IFNULL(tr.target_qty, 0) AS TargetQty,
                    IFNULL(tr.target_cost, 0) AS TargetCost,
                    IFNULL(tr.act_reg_qty, 0) + IFNULL(tr.act_ot_qty, 0) AS ActualQty,
                    IFNULL(tr.act_reg_cost, 0) + IFNULL(tr.act_ot_cost, 0) AS ActualCost,
                    IFNULL(tr.remain_qty, 0) AS RemainingQty,
                    '' AS Role,
                    t.proj_id AS ProjectId,
                    '' AS UnitOfMeasure,
                    IFNULL(tr.cost_per_qty, 0) AS CostPerUnit,
                    r.clndr_id AS CalendarId
                FROM TASKRSRC tr
                INNER JOIN TASK t ON tr.task_id = t.task_id
                LEFT JOIN RSRC r ON tr.rsrc_id = r.rsrc_id
                WHERE t.proj_id = @ProjectId
                ORDER BY t.task_code, r.rsrc_name";

            using (var reader = await ExecuteSQLiteReaderAsync(connectionString, query, CreateSQLiteProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    resources.Add(new ResourceDto
                    {
                        RsrcId = GetInt32(reader, 0),
                        TaskId = GetInt32(reader, 1),
                        TaskStatusCode = GetString(reader, 2),
                        RsrcName = GetString(reader, 3),
                        TaskName = GetString(reader, 4),
                        TaskCode = GetString(reader, 5),
                        RsrcType = GetString(reader, 6),
                        TargetQty = GetDecimal(reader, 7),
                        TargetCost = GetDecimal(reader, 8),
                        ActualQty = GetDecimal(reader, 9),
                        ActualCost = GetDecimal(reader, 10),
                        RemainingQty = GetDecimal(reader, 11),
                        Role = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        UnitOfMeasure = GetString(reader, 14),
                        CostPerUnit = GetDecimal(reader, 15),
                        CalendarId = reader.IsDBNull(16) ? null : (int?)GetInt32(reader, 16)
                    });
                }
            }

            return resources;
        }

        private static async Task<SQLiteDataReader> ExecuteSQLiteReaderAsync(
            string connectionString,
            string query,
            params SQLiteParameter[] parameters)
        {
            var connection = new SQLiteConnection(connectionString);
            var command = new SQLiteCommand(query, connection)
            {
                CommandTimeout = CommandTimeout
            };

            if (parameters != null && parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            return (SQLiteDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        private static SQLiteParameter CreateSQLiteProjectParameter(int projectId)
        {
            return new SQLiteParameter("@ProjectId", DbType.Int32)
            {
                Value = projectId
            };
        }

        #endregion

        #region SQL Server Implementation

        private async Task<List<ProjectDto>> GetProjectsFromSqlServerAsync(string connectionString)
        {
            var projects = new List<ProjectDto>();

            const string query = @"
                SELECT
                    p.proj_id AS ProjectId,
                    ISNULL(p.proj_short_name, '') AS ProjectCode,
                    ISNULL(root.wbs_name, ISNULL(p.proj_short_name, '')) AS ProjectName,
                    p.add_date AS CreatedDate,
                    ISNULL(root.status_code, '') AS Status,
                    '' AS Description,
                    '' AS ProjectManager,
                    p.plan_start_date AS PlannedStartDate,
                    p.plan_end_date AS PlannedFinishDate,
                    CAST(NULL AS datetime) AS ActualStartDate,
                    CAST(NULL AS datetime) AS ActualFinishDate,
                    CAST(0 AS decimal(18, 4)) AS CompletionPercentage
                FROM PROJECT p
                LEFT JOIN PROJWBS root ON root.proj_id = p.proj_id AND root.proj_node_flag = 'Y'
                ORDER BY ProjectName";

            using (var reader = await ExecuteSqlReaderAsync(connectionString, query))
            {
                while (await reader.ReadAsync())
                {
                    projects.Add(new ProjectDto
                    {
                        ProjectId = GetInt32(reader, 0),
                        ProjectCode = GetString(reader, 1),
                        ProjectName = GetString(reader, 2),
                        CreatedDate = GetDateTime(reader, 3) ?? DateTime.MinValue,
                        Status = GetString(reader, 4),
                        Description = GetString(reader, 5),
                        ProjectManager = GetString(reader, 6),
                        PlannedStartDate = GetDateTime(reader, 7),
                        PlannedFinishDate = GetDateTime(reader, 8),
                        ActualStartDate = GetDateTime(reader, 9),
                        ActualFinishDate = GetDateTime(reader, 10),
                        CompletionPercentage = GetDecimal(reader, 11)
                    });
                }
            }

            return projects;
        }

        private async Task<List<ActivityDto>> GetActivitiesFromSqlServerAsync(string connectionString, int projectId)
        {
            var activities = new List<ActivityDto>();

            const string query = @"
                SELECT
                    t.task_id AS TaskId,
                    ISNULL(t.task_code, '') AS TaskCode,
                    ISNULL(t.task_name, '') AS TaskName,
                    ISNULL(t.status_code, '') AS StatusCode,
                    ISNULL(t.wbs_id, 0) AS WbsId,
                    ISNULL(w.wbs_name, '') AS WbsFullName,
                    ISNULL(t.phys_complete_pct, 0) AS CompletePct,
                    t.act_start_date AS ActStartDate,
                    t.act_end_date AS ActEndDate,
                    t.target_start_date AS PlanStartDate,
                    t.target_end_date AS PlanEndDate,
                    ISNULL(t.target_drtn_hr_cnt, 0) AS Duration,
                    ISNULL(t.task_type, '') AS TaskType,
                    '' AS ResourceName,
                    ISNULL(t.remain_drtn_hr_cnt, 0) AS RemainingDuration,
                    ISNULL(t.target_drtn_hr_cnt, 0) - ISNULL(t.remain_drtn_hr_cnt, 0) AS ActualDuration,
                    ISNULL(t.free_float_hr_cnt, 0) AS FreeFloat,
                    ISNULL(t.total_float_hr_cnt, 0) AS TotalFloat,
                    t.proj_id AS ProjectId
                FROM TASK t
                LEFT JOIN PROJWBS w ON t.wbs_id = w.wbs_id
                WHERE t.proj_id = @ProjectId
                ORDER BY t.task_code";

            using (var reader = await ExecuteSqlReaderAsync(connectionString, query, CreateProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    activities.Add(new ActivityDto
                    {
                        TaskId = GetInt32(reader, 0),
                        TaskCode = GetString(reader, 1),
                        TaskName = GetString(reader, 2),
                        StatusCode = GetString(reader, 3),
                        WbsId = GetInt32(reader, 4),
                        WbsFullName = GetString(reader, 5),
                        CompletePct = GetDecimal(reader, 6),
                        ActStartDate = GetDateTime(reader, 7),
                        ActEndDate = GetDateTime(reader, 8),
                        PlanStartDate = GetDateTime(reader, 9),
                        PlanEndDate = GetDateTime(reader, 10),
                        Duration = HoursToDays(GetDecimal(reader, 11)),
                        TaskType = GetString(reader, 12),
                        ResourceName = GetString(reader, 13),
                        RemainingDuration = HoursToDays(GetDecimal(reader, 14)),
                        ActualDuration = HoursToDays(GetDecimal(reader, 15)),
                        FreeFloat = HoursToDays(GetDecimal(reader, 16)),
                        TotalFloat = HoursToDays(GetDecimal(reader, 17)),
                        ProjectId = GetInt32(reader, 18)
                    });
                }
            }

            return activities;
        }

        private async Task<List<RelationshipDto>> GetRelationshipsFromSqlServerAsync(string connectionString, int projectId)
        {
            var relationships = new List<RelationshipDto>();

            const string query = @"
                SELECT
                    tp.task_pred_id AS TaskPredId,
                    tp.pred_task_id AS PredTaskId,
                    tp.task_id AS TaskId,
                    ISNULL(tp.pred_type, '') AS PredType,
                    ISNULL(pred.status_code, '') AS PredTaskStatusCode,
                    ISNULL(succ.status_code, '') AS TaskStatusCode,
                    ISNULL(predwbs.wbs_name, '') AS PredTaskWbsFullName,
                    ISNULL(succwbs.wbs_name, '') AS TaskWbsFullName,
                    ISNULL(tp.lag_hr_cnt, 0) AS LagHrCnt,
                    ISNULL(pred.task_name, '') AS PredTaskName,
                    ISNULL(succ.task_name, '') AS TaskName,
                    ISNULL(pred.task_code, '') AS PredTaskCode,
                    ISNULL(succ.task_code, '') AS TaskCode,
                    succ.proj_id AS ProjectId,
                    'Hours' AS LagType
                FROM TASKPRED tp
                INNER JOIN TASK succ ON tp.task_id = succ.task_id
                LEFT JOIN TASK pred ON tp.pred_task_id = pred.task_id
                LEFT JOIN PROJWBS predwbs ON pred.wbs_id = predwbs.wbs_id
                LEFT JOIN PROJWBS succwbs ON succ.wbs_id = succwbs.wbs_id
                WHERE succ.proj_id = @ProjectId
                ORDER BY pred.task_code, succ.task_code";

            using (var reader = await ExecuteSqlReaderAsync(connectionString, query, CreateProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    relationships.Add(new RelationshipDto
                    {
                        TaskPredId = GetInt32(reader, 0),
                        PredTaskId = GetInt32(reader, 1),
                        TaskId = GetInt32(reader, 2),
                        PredType = GetString(reader, 3),
                        PredTaskStatusCode = GetString(reader, 4),
                        TaskStatusCode = GetString(reader, 5),
                        PredTaskWbsFullName = GetString(reader, 6),
                        TaskWbsFullName = GetString(reader, 7),
                        LagHrCnt = GetDecimal(reader, 8),
                        PredTaskName = GetString(reader, 9),
                        TaskName = GetString(reader, 10),
                        PredTaskCode = GetString(reader, 11),
                        TaskCode = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        LagType = GetString(reader, 14)
                    });
                }
            }

            return relationships;
        }

        private async Task<List<ResourceDto>> GetResourcesFromSqlServerAsync(string connectionString, int projectId)
        {
            var resources = new List<ResourceDto>();

            const string query = @"
                SELECT
                    tr.taskrsrc_id AS RsrcId,
                    tr.task_id AS TaskId,
                    ISNULL(t.status_code, '') AS TaskStatusCode,
                    ISNULL(r.rsrc_name, '') AS RsrcName,
                    ISNULL(t.task_name, '') AS TaskName,
                    ISNULL(t.task_code, '') AS TaskCode,
                    ISNULL(r.rsrc_type, '') AS RsrcType,
                    ISNULL(tr.target_qty, 0) AS TargetQty,
                    ISNULL(tr.target_cost, 0) AS TargetCost,
                    ISNULL(tr.act_reg_qty, 0) + ISNULL(tr.act_ot_qty, 0) AS ActualQty,
                    ISNULL(tr.act_reg_cost, 0) + ISNULL(tr.act_ot_cost, 0) AS ActualCost,
                    ISNULL(tr.remain_qty, 0) AS RemainingQty,
                    '' AS Role,
                    t.proj_id AS ProjectId,
                    '' AS UnitOfMeasure,
                    ISNULL(tr.cost_per_qty, 0) AS CostPerUnit,
                    r.clndr_id AS CalendarId
                FROM TASKRSRC tr
                INNER JOIN TASK t ON tr.task_id = t.task_id
                LEFT JOIN RSRC r ON tr.rsrc_id = r.rsrc_id
                WHERE t.proj_id = @ProjectId
                ORDER BY t.task_code, r.rsrc_name";

            using (var reader = await ExecuteSqlReaderAsync(connectionString, query, CreateProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    resources.Add(new ResourceDto
                    {
                        RsrcId = GetInt32(reader, 0),
                        TaskId = GetInt32(reader, 1),
                        TaskStatusCode = GetString(reader, 2),
                        RsrcName = GetString(reader, 3),
                        TaskName = GetString(reader, 4),
                        TaskCode = GetString(reader, 5),
                        RsrcType = GetString(reader, 6),
                        TargetQty = GetDecimal(reader, 7),
                        TargetCost = GetDecimal(reader, 8),
                        ActualQty = GetDecimal(reader, 9),
                        ActualCost = GetDecimal(reader, 10),
                        RemainingQty = GetDecimal(reader, 11),
                        Role = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        UnitOfMeasure = GetString(reader, 14),
                        CostPerUnit = GetDecimal(reader, 15),
                        CalendarId = reader.IsDBNull(16) ? null : (int?)GetInt32(reader, 16)
                    });
                }
            }

            return resources;
        }

        private static async Task<SqlDataReader> ExecuteSqlReaderAsync(
            string connectionString,
            string query,
            params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection)
            {
                CommandTimeout = CommandTimeout
            };

            if (parameters != null && parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        private static SqlParameter CreateProjectParameter(int projectId)
        {
            return new SqlParameter("@ProjectId", SqlDbType.Int)
            {
                Value = projectId
            };
        }

        #endregion

        #region Oracle Implementation

        private async Task<bool> TestOracleConnectionAsync(string connectionString)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
        }

        private async Task<List<ProjectDto>> GetProjectsFromOracleAsync(string connectionString)
        {
            var projects = new List<ProjectDto>();

            const string query = @"
                SELECT
                    p.proj_id AS ProjectId,
                    NVL(p.proj_short_name, '') AS ProjectCode,
                    NVL(root.wbs_name, NVL(p.proj_short_name, '')) AS ProjectName,
                    p.add_date AS CreatedDate,
                    NVL(root.status_code, '') AS Status,
                    '' AS Description,
                    '' AS ProjectManager,
                    p.plan_start_date AS PlannedStartDate,
                    p.plan_end_date AS PlannedFinishDate,
                    CAST(NULL AS DATE) AS ActualStartDate,
                    CAST(NULL AS DATE) AS ActualFinishDate,
                    CAST(0 AS NUMBER(18, 4)) AS CompletionPercentage
                FROM PROJECT p
                LEFT JOIN PROJWBS root ON root.proj_id = p.proj_id AND root.proj_node_flag = 'Y'
                ORDER BY ProjectName";

            using (var reader = await ExecuteOracleReaderAsync(connectionString, query))
            {
                while (await reader.ReadAsync())
                {
                    projects.Add(new ProjectDto
                    {
                        ProjectId = GetInt32(reader, 0),
                        ProjectCode = GetString(reader, 1),
                        ProjectName = GetString(reader, 2),
                        CreatedDate = GetDateTime(reader, 3) ?? DateTime.MinValue,
                        Status = GetString(reader, 4),
                        Description = GetString(reader, 5),
                        ProjectManager = GetString(reader, 6),
                        PlannedStartDate = GetDateTime(reader, 7),
                        PlannedFinishDate = GetDateTime(reader, 8),
                        ActualStartDate = GetDateTime(reader, 9),
                        ActualFinishDate = GetDateTime(reader, 10),
                        CompletionPercentage = GetDecimal(reader, 11)
                    });
                }
            }

            return projects;
        }

        private async Task<List<ActivityDto>> GetActivitiesFromOracleAsync(string connectionString, int projectId)
        {
            var activities = new List<ActivityDto>();

            const string query = @"
                SELECT
                    t.task_id AS TaskId,
                    NVL(t.task_code, '') AS TaskCode,
                    NVL(t.task_name, '') AS TaskName,
                    NVL(t.status_code, '') AS StatusCode,
                    NVL(t.wbs_id, 0) AS WbsId,
                    NVL(w.wbs_name, '') AS WbsFullName,
                    NVL(t.phys_complete_pct, 0) AS CompletePct,
                    t.act_start_date AS ActStartDate,
                    t.act_end_date AS ActEndDate,
                    t.target_start_date AS PlanStartDate,
                    t.target_end_date AS PlanEndDate,
                    NVL(t.target_drtn_hr_cnt, 0) AS Duration,
                    NVL(t.task_type, '') AS TaskType,
                    '' AS ResourceName,
                    NVL(t.remain_drtn_hr_cnt, 0) AS RemainingDuration,
                    NVL(t.target_drtn_hr_cnt, 0) - NVL(t.remain_drtn_hr_cnt, 0) AS ActualDuration,
                    NVL(t.free_float_hr_cnt, 0) AS FreeFloat,
                    NVL(t.total_float_hr_cnt, 0) AS TotalFloat,
                    t.proj_id AS ProjectId
                FROM TASK t
                LEFT JOIN PROJWBS w ON t.wbs_id = w.wbs_id
                WHERE t.proj_id = :ProjectId
                ORDER BY t.task_code";

            using (var reader = await ExecuteOracleReaderAsync(connectionString, query, CreateOracleProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    activities.Add(new ActivityDto
                    {
                        TaskId = GetInt32(reader, 0),
                        TaskCode = GetString(reader, 1),
                        TaskName = GetString(reader, 2),
                        StatusCode = GetString(reader, 3),
                        WbsId = GetInt32(reader, 4),
                        WbsFullName = GetString(reader, 5),
                        CompletePct = GetDecimal(reader, 6),
                        ActStartDate = GetDateTime(reader, 7),
                        ActEndDate = GetDateTime(reader, 8),
                        PlanStartDate = GetDateTime(reader, 9),
                        PlanEndDate = GetDateTime(reader, 10),
                        Duration = HoursToDays(GetDecimal(reader, 11)),
                        TaskType = GetString(reader, 12),
                        ResourceName = GetString(reader, 13),
                        RemainingDuration = HoursToDays(GetDecimal(reader, 14)),
                        ActualDuration = HoursToDays(GetDecimal(reader, 15)),
                        FreeFloat = HoursToDays(GetDecimal(reader, 16)),
                        TotalFloat = HoursToDays(GetDecimal(reader, 17)),
                        ProjectId = GetInt32(reader, 18)
                    });
                }
            }

            return activities;
        }

        private async Task<List<RelationshipDto>> GetRelationshipsFromOracleAsync(string connectionString, int projectId)
        {
            var relationships = new List<RelationshipDto>();

            const string query = @"
                SELECT
                    tp.task_pred_id AS TaskPredId,
                    tp.pred_task_id AS PredTaskId,
                    tp.task_id AS TaskId,
                    NVL(tp.pred_type, '') AS PredType,
                    NVL(pred.status_code, '') AS PredTaskStatusCode,
                    NVL(succ.status_code, '') AS TaskStatusCode,
                    NVL(predwbs.wbs_name, '') AS PredTaskWbsFullName,
                    NVL(succwbs.wbs_name, '') AS TaskWbsFullName,
                    NVL(tp.lag_hr_cnt, 0) AS LagHrCnt,
                    NVL(pred.task_name, '') AS PredTaskName,
                    NVL(succ.task_name, '') AS TaskName,
                    NVL(pred.task_code, '') AS PredTaskCode,
                    NVL(succ.task_code, '') AS TaskCode,
                    succ.proj_id AS ProjectId,
                    'Hours' AS LagType
                FROM TASKPRED tp
                INNER JOIN TASK succ ON tp.task_id = succ.task_id
                LEFT JOIN TASK pred ON tp.pred_task_id = pred.task_id
                LEFT JOIN PROJWBS predwbs ON pred.wbs_id = predwbs.wbs_id
                LEFT JOIN PROJWBS succwbs ON succ.wbs_id = succwbs.wbs_id
                WHERE succ.proj_id = :ProjectId
                ORDER BY pred.task_code, succ.task_code";

            using (var reader = await ExecuteOracleReaderAsync(connectionString, query, CreateOracleProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    relationships.Add(new RelationshipDto
                    {
                        TaskPredId = GetInt32(reader, 0),
                        PredTaskId = GetInt32(reader, 1),
                        TaskId = GetInt32(reader, 2),
                        PredType = GetString(reader, 3),
                        PredTaskStatusCode = GetString(reader, 4),
                        TaskStatusCode = GetString(reader, 5),
                        PredTaskWbsFullName = GetString(reader, 6),
                        TaskWbsFullName = GetString(reader, 7),
                        LagHrCnt = GetDecimal(reader, 8),
                        PredTaskName = GetString(reader, 9),
                        TaskName = GetString(reader, 10),
                        PredTaskCode = GetString(reader, 11),
                        TaskCode = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        LagType = GetString(reader, 14)
                    });
                }
            }

            return relationships;
        }

        private async Task<List<ResourceDto>> GetResourcesFromOracleAsync(string connectionString, int projectId)
        {
            var resources = new List<ResourceDto>();

            const string query = @"
                SELECT
                    tr.taskrsrc_id AS RsrcId,
                    tr.task_id AS TaskId,
                    NVL(t.status_code, '') AS TaskStatusCode,
                    NVL(r.rsrc_name, '') AS RsrcName,
                    NVL(t.task_name, '') AS TaskName,
                    NVL(t.task_code, '') AS TaskCode,
                    NVL(r.rsrc_type, '') AS RsrcType,
                    NVL(tr.target_qty, 0) AS TargetQty,
                    NVL(tr.target_cost, 0) AS TargetCost,
                    NVL(tr.act_reg_qty, 0) + NVL(tr.act_ot_qty, 0) AS ActualQty,
                    NVL(tr.act_reg_cost, 0) + NVL(tr.act_ot_cost, 0) AS ActualCost,
                    NVL(tr.remain_qty, 0) AS RemainingQty,
                    '' AS Role,
                    t.proj_id AS ProjectId,
                    '' AS UnitOfMeasure,
                    NVL(tr.cost_per_qty, 0) AS CostPerUnit,
                    r.clndr_id AS CalendarId
                FROM TASKRSRC tr
                INNER JOIN TASK t ON tr.task_id = t.task_id
                LEFT JOIN RSRC r ON tr.rsrc_id = r.rsrc_id
                WHERE t.proj_id = :ProjectId
                ORDER BY t.task_code, r.rsrc_name";

            using (var reader = await ExecuteOracleReaderAsync(connectionString, query, CreateOracleProjectParameter(projectId)))
            {
                while (await reader.ReadAsync())
                {
                    resources.Add(new ResourceDto
                    {
                        RsrcId = GetInt32(reader, 0),
                        TaskId = GetInt32(reader, 1),
                        TaskStatusCode = GetString(reader, 2),
                        RsrcName = GetString(reader, 3),
                        TaskName = GetString(reader, 4),
                        TaskCode = GetString(reader, 5),
                        RsrcType = GetString(reader, 6),
                        TargetQty = GetDecimal(reader, 7),
                        TargetCost = GetDecimal(reader, 8),
                        ActualQty = GetDecimal(reader, 9),
                        ActualCost = GetDecimal(reader, 10),
                        RemainingQty = GetDecimal(reader, 11),
                        Role = GetString(reader, 12),
                        ProjectId = GetInt32(reader, 13),
                        UnitOfMeasure = GetString(reader, 14),
                        CostPerUnit = GetDecimal(reader, 15),
                        CalendarId = reader.IsDBNull(16) ? null : (int?)GetInt32(reader, 16)
                    });
                }
            }

            return resources;
        }

        #endregion

        private static async Task<OracleDataReader> ExecuteOracleReaderAsync(
            string connectionString,
            string query,
            params OracleParameter[] parameters)
        {
            var connection = new OracleConnection(connectionString);
            var command = new OracleCommand(query, connection)
            {
                BindByName = true,
                CommandTimeout = CommandTimeout
            };

            if (parameters != null && parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        private static OracleParameter CreateOracleProjectParameter(int projectId)
        {
            return new OracleParameter("ProjectId", OracleDbType.Int32)
            {
                Value = projectId
            };
        }

        private static string GetString(IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal));
        }

        private static int GetInt32(IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static decimal GetDecimal(IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToDecimal(reader.GetValue(ordinal));
        }

        private static DateTime? GetDateTime(IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (DateTime?)Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static int HoursToDays(decimal hours)
        {
            return Convert.ToInt32(Math.Round(hours / 8m, MidpointRounding.AwayFromZero));
        }
    }
}
