using Fantasy._Frontend.Repositories;
using Fantasy.Shared.Entities;
using Microsoft.AspNetCore.Components;

namespace Fantasy._Frontend.Pages.Countries
{
    public partial class CountriesIndex
    {
        //De esta forma inyectamos el servicio del IRepository que esta en el program del Frontend
        [Inject] private IRepository Repository { get; set; } = null!;

        private List<Country>? Countries { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var responseHppt = await Repository.GetAsync<List<Country>>("api/countries");
            Countries = responseHppt.Response!;
        }
    }
}