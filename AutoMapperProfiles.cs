using System;
using AutoMapper;
using FileSharing.Data;
using FileSharing.Models;
namespace FileSharing
{
    public class UploadProfile : Profile
    {
        public UploadProfile()
        {
            CreateMap<InputUpload, Uploads>()
            .ForMember(u => u.id, op => op.Ignore())
            .ForMember(u => u.UploadTime , op => op.Ignore());
            

            CreateMap<Uploads, UploadViewModel>();
        }
    }
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Data.ApplicationUser, UserViewModel>();
        }
    }
}