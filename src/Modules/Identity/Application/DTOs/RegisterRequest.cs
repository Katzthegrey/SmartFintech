using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? Phone = null,
    string? FirstName = null,
    string? LastName = null,
    DateTime? DateOfBirth = null,
    string? Address = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,
    decimal? AnnualIncome = null,
    string? EmploymentStatus = null,
    string? TaxIdNumber = null,
    string RegistrationType = "client" // client, investor, business
);