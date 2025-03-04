//using Xunit;
//using System.Collections.Generic;

//namespace ClinicAssistant.Tests
//{
//    public class DatabaseFacadeTests
//    {
//        [Fact]
//        public void AddPatient_ValidInput_ReturnsPatientID()
//        {
//            var dbFacade = new DatabaseFacade();
//            string fullName = "Иван Иванов";
//            int age = 30;
//            string gender = "Мужской";
//            int patientId = dbFacade.AddNewPatient(fullName, age, gender);
//            Assert.True(patientId > 0, "Пациент должен быть успешно добавлен с валидным ID.");
//        }

//        [Fact]
//        public void GetSymptoms_SearchParameter_ReturnsFilteredResults()
//        {
//            var dbFacade = new DatabaseFacade();
//            string search = "Головная боль";
//            var expectedSymptoms = new List<(int SymptomID, string SymptomName)>
//            {
//                (1, "Головная боль"),
//                (2, "Мигрень")
//            };

//            var symptoms = dbFacade.GetSymptoms(search);

//            Assert.NotEmpty(symptoms);
//            Assert.Equal(expectedSymptoms[0].SymptomName, symptoms[0].SymptomName);
//        }

//        [Fact]
//        public void AddPatientSymptoms_ValidData_StoresSymptomsCorrectly()
//        {
//            var dbFacade = new DatabaseFacade();
//            int patientId = 1;
//            var symptomIds = new List<int> { 1, 2, 3 };
//            var exception = Record.Exception(() => dbFacade.AddPatientSymptoms(patientId, symptomIds));
//            Assert.Null(exception);
//        }

//        [Fact]
//        public async System.Threading.Tasks.Task GetDiagnosesAsync_ValidPatientId_ReturnsDiagnoses()
//        {
//            var dbFacade = new DatabaseFacade();
//            int patientId = 1;
//            var diagnoses = await dbFacade.GetDiagnosesAsync(patientId);
//            Assert.NotNull(diagnoses);
//            Assert.NotEmpty(diagnoses);
//        }
//    }
//}