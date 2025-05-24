namespace Ava.API.Interfaces;
public interface IAmadeusFlightSearchService
{
    Task<AmadeusFlightOfferSearchResult> GetFlightOffersAsync(FlightOfferSearchRequestDTO searchRequestDTO);
}
