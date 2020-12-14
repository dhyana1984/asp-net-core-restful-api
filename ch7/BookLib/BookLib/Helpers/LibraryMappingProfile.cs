using System;
using AutoMapper;
using BookLib.Entities;
using BookLib.Models;
using Library.API.Extentions;

namespace BookLib.Helpers
{
    public class LibraryMappingProfile : Profile
    {
       public LibraryMappingProfile()
        {
            CreateMap<Author, AuthorDto>().ForMember(dest => dest.Age, config =>
            {
                config.MapFrom(src => src.BirthDate.GetCurrentAge());
            });
            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<AuthorForUpdateDto, Author>();

            CreateMap<Book, BookDto>();
            CreateMap<BookForCreationDto, Book>();
            CreateMap<BookForUpdateDto, Book>();
        }
    }
}
