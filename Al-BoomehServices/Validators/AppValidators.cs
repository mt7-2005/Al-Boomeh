using Al_BoomehDAL.Classes;
using Al_BoomehServices.Services;
using FluentValidation;
using System;

namespace Al_BoomehServices.Validators
{
    public class AppValidators
    {
       

        public static bool BeAValidCoordinate(string? value)
        {
            return double.TryParse(value, out _);
        }
    }
}