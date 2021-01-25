using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorIndexedDbJsClientDemo.Data;
using System.Linq;
using BlazorIndexedDbJs;

namespace BlazorIndexedDbJsClientDemo.Pages
{
    public partial class IndexedDbTest: ComponentBase, IDisposable
    {
        [Inject]
        public IndexedDBManager DbManager {get; set; }

        [Inject]
        public DbStore DbStore {get; set; }

        private string Message { get; set; }
        private IList<Person> People { get; set; } = new List<Person>();
        private Person CurrentPerson { get; set; } = new Person();
        private string SearchFirstName;

        protected override void OnInitialized()
        {
            DbManager.ActionCompleted += OnIndexedDbNotification;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GetRecords();
            }
        }

        public void Dispose()
        {
            DbManager.ActionCompleted -= OnIndexedDbNotification;
        }

        private async Task GetRecords(string firstName = "")
        {
            IList<Person> results;

            if (String.IsNullOrEmpty(firstName))
            {
                results = await DbManager.GetRecords<Person>(DbStoreConfig.Employees);
            }
            else
            {
                var indexSearch = new StoreIndexQuery<string>
                {
                    Storename = DbStoreConfig.Employees,
                    IndexName = "firstName",
                    QueryValue = firstName,
                };

                results = await DbManager.GetAllRecordsByIndex<string, Person>(indexSearch);
            }


            if (results != null && results.Any())
            {
                People = results;
            }
            else
            {
                People.Clear();
                Message = "No Records found";
            }

            StateHasChanged();
        }

        private async Task EditPerson(long id)
        {
            try
            {
                CurrentPerson = await DbManager.GetRecordById<long, Person>(DbStoreConfig.Employees, id);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        private async void AddRecord()
        {
            if (CurrentPerson.Id.HasValue)
            {
                await DbManager.UpdateRecord(DbStoreConfig.Employees, CurrentPerson);
            }
            else
            {
                await DbManager.AddRecord(DbStoreConfig.Employees, CurrentPerson);
            }

            CurrentPerson = new Person();

            await GetRecords();
        }

        private async void AddRecords()
        {
            var list = new List<Person>();

            for (int i = 0; i < 100; i++)
            {
                var person = new Person() {
                    FirstName = "person "+i.ToString(),
                    LastName = "lastname "+i.ToString()
                };

                list.Add(person);
            }

            await DbManager.AddRecords<Person>(DbStoreConfig.Employees, list.ToArray());

            await GetRecords();
        }

        private async Task DeleteRecord(long? id)
        {
            await DbManager.DeleteRecord(DbStoreConfig.Employees, id);

            await GetRecords();
        }

        private async Task ClearStore()
        {
            await DbManager.ClearStore(DbStoreConfig.Employees);

            await GetRecords();
        }

        private async Task SearchRecords()
        {
            await GetRecords(SearchFirstName);
        }

        private void OnIndexedDbNotification(object sender, IndexedDBNotificationArgs args)
        {
            Message = args.Message;

            StateHasChanged();
        }
    }
}