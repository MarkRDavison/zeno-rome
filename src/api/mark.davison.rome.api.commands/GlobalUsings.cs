global using mark.davison.common.abstractions.Services;
global using mark.davison.common.authentication.server.abstractions.Services;
global using mark.davison.common.CQRS;
global using mark.davison.common.persistence;
global using mark.davison.common.server.abstractions.CQRS;
global using mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;
global using mark.davison.rome.api.models.Entities;
global using mark.davison.rome.api.persistence;
global using mark.davison.rome.shared.accounting.constants;
global using mark.davison.rome.shared.accounting.rules;
global using mark.davison.rome.shared.models.dto.Scenarios.Commands.CreateTransaction;
global using mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertAccount;
global using mark.davison.rome.shared.models.dto.Shared;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("mark.davison.rome.api.commands.tests")]