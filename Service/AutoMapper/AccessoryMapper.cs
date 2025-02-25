using AutoMapper;
using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.AutoMapper;
public partial class MapperConfig : Profile
{
    partial void AddAccessoryMapperConfig()
    {
        CreateMap<Accessory, AccessoryResponse>().ReverseMap();
        CreateMap<Accessory, AccessoryRequest>().ReverseMap();
    }

}
