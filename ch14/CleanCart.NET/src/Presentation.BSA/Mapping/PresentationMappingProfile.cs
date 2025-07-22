using Application.Operations.UseCases.ProcessPayment;
using AutoMapper;
using Presentation.BSA.Models.ViewModels;

namespace Presentation.BSA.Mapping;

public class PresentationMappingProfile : Profile
{
    public PresentationMappingProfile()
    {
        CreateMap<CheckoutViewModel, ProcessPaymentCommand>();
    }
}