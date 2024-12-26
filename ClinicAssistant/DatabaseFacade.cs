using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using static ClinicAssistant.FollowUpQuestionsWindow;

namespace ClinicAssistant
{
    public class DatabaseFacade
    {
        //private readonly string connectionString = "data source=192.168.147.54;initial catalog=PomoshnikPolicliniki;persist security info=True;user id=is;password=1;MultipleActiveResultSets=True;App=EntityFramework";

        private readonly string connectionString = "data source=localhost;initial catalog=PomoshnikPolicliniki8;integrated security=True;encrypt=False;MultipleActiveResultSets=True;";

        public int AddPatient(string fullName, int age, string gender)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Patients (FullName, Age, Gender) OUTPUT INSERTED.PatientID VALUES (@FullName, @Age, @Gender)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Gender", gender);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public List<(int SymptomID, string SymptomName)> GetSymptoms(string search = "")
        {
            var symptoms = new List<(int, string)>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SymptomID, SymptomName FROM Symptoms";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE SymptomName LIKE @search";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            symptoms.Add((reader.GetInt32(0), reader.GetString(1)));
                        }
                    }
                }
            }

            return symptoms;
        }

        public void AddPatientSymptoms(int patientId, IEnumerable<int> symptomIds)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var symptomId in symptomIds)
                        {
                            string query = "INSERT INTO PatientSymptoms (PatientID, SymptomID) VALUES (@PatientID, @SymptomID)";
                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@PatientID", patientId);
                                command.Parameters.AddWithValue("@SymptomID", symptomId);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<int> GetPatientSymptoms(int patientId)
        {
            var symptomIds = new List<int>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SymptomID FROM PatientSymptoms WHERE PatientID = @PatientID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patientId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            symptomIds.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            return symptomIds;
        }

        public List<QuestionModel> GetQuestionsBySymptoms(IEnumerable<int> symptomIds)
        {
            var questions = new List<QuestionModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"
                    SELECT fq.QuestionID, fq.SymptomID, s.SymptomName, fq.Question
                    FROM FollowUpQuestions fq
                    JOIN Symptoms s ON fq.SymptomID = s.SymptomID
                    WHERE fq.SymptomID IN ({string.Join(",", symptomIds)})";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questions.Add(new QuestionModel
                        {
                            QuestionID = reader.GetInt32(0),
                            SymptomID = reader.GetInt32(1),
                            SymptomName = reader.GetString(2),
                            QuestionText = reader.GetString(3)
                        });
                    }
                }
            }

            return questions;
        }

        public List<AnswerModel> GetAnswersByQuestion(int questionId)
        {
            var answers = new List<AnswerModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT AnswerID, Answer FROM Answers WHERE QuestionID = @QuestionID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@QuestionID", questionId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            answers.Add(new AnswerModel
                            {
                                AnswerID = reader.GetInt32(0),
                                AnswerText = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return answers;
        }

        public void SavePatientAnswers(int patientId, IEnumerable<int> answerIds)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = "INSERT INTO PatientAnswers (PatientID, AnswerID) VALUES (@PatientID, @AnswerID)";
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.Add("@PatientID", System.Data.SqlDbType.Int).Value = patientId;
                            command.Parameters.Add("@AnswerID", System.Data.SqlDbType.Int);

                            foreach (var answerId in answerIds)
                            {
                                command.Parameters["@AnswerID"].Value = answerId;
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<List<(int DiagnosisID, string DiagnosisName, int Matches)>> GetDiagnosesAsync(int patientId)
        {
            var diagnoses = new List<(int DiagnosisID, string DiagnosisName, int Matches)>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT d.DiagnosisID, d.DiagnosisName, COUNT(pa.AnswerID) AS AnswerMatches
                FROM PatientAnswers pa
                JOIN AnswerDiagnoses ad ON pa.AnswerID = ad.AnswerID
                JOIN Diagnoses d ON ad.DiagnosisID = d.DiagnosisID
                WHERE pa.PatientID = @PatientID
                GROUP BY d.DiagnosisID, d.DiagnosisName
                ORDER BY AnswerMatches DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patientId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            diagnoses.Add((
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetInt32(2)
                            ));
                        }
                    }
                }
            }

            return diagnoses;
        }
        public async Task ClearExistingDiagnosesAsync(int patientId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM PatientDiagnoses WHERE PatientID = @PatientID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patientId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SaveDiagnosisAsync(int patientId, int diagnosisId, int percentage)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                INSERT INTO PatientDiagnoses (PatientID, DiagnosisID, Percentageofdiagnosis)
                VALUES (@PatientID, @DiagnosisID, @Percentageofdiagnosis)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patientId);
                    command.Parameters.AddWithValue("@DiagnosisID", diagnosisId);
                    command.Parameters.AddWithValue("@Percentageofdiagnosis", percentage);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<(string FullName, string OfficeNumber)>> GetDoctorsForDiagnosisAsync(int diagnosisId)
        {
            var doctors = new List<(string FullName, string OfficeNumber)>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT doc.FullName, doc.OfficeNumber
                FROM DoctorDiagnoses dd
                JOIN Doctors doc ON dd.DoctorID = doc.DoctorID
                WHERE dd.DiagnosisID = @DiagnosisID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DiagnosisID", diagnosisId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            doctors.Add((
                                reader.GetString(0),
                                reader.GetString(1)
                            ));
                        }
                    }
                }
            }

            return doctors;
        }

        public DataTable GetDataTable(string query, Dictionary<string, object> parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }
        public async Task ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task> operation)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await operation(connection, transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}