using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicAssistant; // �������� �� �������� ������������ ��� ������ ��������� �������

namespace TestProject
{
    public class DatabaseFacadeTests
    {
        private readonly DatabaseFacade dbFacade;

        public DatabaseFacadeTests()
        {
            dbFacade = new DatabaseFacade();
        }

        [Fact]
        public void AddPatient_ValidInput_ReturnsPatientID()
        {
            // Arrange
            string fullName = "���� ������";
            int age = 30;
            string gender = "�������";

            // Act
            int patientId = dbFacade.AddPatient(fullName, age, gender);

            // Assert
            Assert.True(patientId > 0, "������� ������ ���� ������� �������� � �������� ID.");
        }

        [Fact]
        public void GetSymptoms_SearchParameter_ReturnsFilteredResults()
        {
            // Arrange
            string search = "�������� ����";
            var expectedSymptoms = new List<(int SymptomID, string SymptomName)>
            {
                (1, "�������� ����"),
                (2, "�������")
            };

            // Act
            var symptoms = dbFacade.GetSymptoms(search);

            // Assert
            Assert.NotEmpty(symptoms);
            Assert.Equal(expectedSymptoms[0].SymptomName, symptoms[0].SymptomName);
        }

        [Fact]
        public void AddPatientSymptoms_ValidData_StoresSymptomsCorrectly()
        {
            // Arrange
            int patientId = 1;
            var symptomIds = new List<int> { 1, 2, 3 };

            // Act
            var exception = Record.Exception(() => dbFacade.AddPatientSymptoms(patientId, symptomIds));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetDiagnosesAsync_ValidPatientId_ReturnsDiagnoses()
        {
            // Arrange
            int patientId = 1;

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                var diagnoses = await dbFacade.GetDiagnosesAsync(patientId);

                // Assert
                Assert.NotNull(diagnoses);
                Assert.NotEmpty(diagnoses);
            });

            Assert.Null(exception);
        }

        [Fact]
        public async Task SaveDiagnosisAsync_ValidData_StoresDiagnosisSuccessfully()
        {
            // Arrange
            int patientId = 1;
            int diagnosisId = 101;
            int percentage = 80;

            // Act
            var exception = await Record.ExceptionAsync(() =>
                dbFacade.SaveDiagnosisAsync(patientId, diagnosisId, percentage)
            );

            // Assert
            Assert.Null(exception);
        }
    }
}
