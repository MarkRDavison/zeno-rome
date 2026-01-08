namespace mark.davison.rome.shared.accounting.rules;

public static class AllowableSourceDestinationAccounts
{
    private static IDictionary<Guid, IDictionary<Guid, HashSet<Guid>>> _allowableSourceDestinations;

    public static bool IsValidSourceAndDestinationAccount(Guid transactionType, Guid sourceAccountId, Guid destinationAccountId)
    {
        if (_allowableSourceDestinations.TryGetValue(transactionType, out var sourceDestinations))
        {
            if (sourceDestinations.TryGetValue(sourceAccountId, out var destinations))
            {
                return destinations.Contains(destinationAccountId);
            }
        }

        return false;
    }

    public static HashSet<Guid> GetSourceAccountTypes(Guid transactionTypeId)
    {
        if (!_allowableSourceDestinations.ContainsKey(transactionTypeId))
        {
            return new HashSet<Guid>();
        }

        return _allowableSourceDestinations[transactionTypeId].Keys.ToHashSet();
    }

    public static HashSet<Guid> GetDestinationAccountTypes(Guid transactionTypeId)
    {
        if (!_allowableSourceDestinations.ContainsKey(transactionTypeId))
        {
            return new HashSet<Guid>();
        }

        return _allowableSourceDestinations[transactionTypeId].Values.SelectMany(_ => _.AsEnumerable()).ToHashSet();
    }
    public static HashSet<Guid> GetDestinationAccountTypesForSource(Guid transactionTypeId, Guid sourceAccountTypeId)
    {
        if (!_allowableSourceDestinations.ContainsKey(transactionTypeId))
        {
            return new HashSet<Guid>();
        }

        return _allowableSourceDestinations[transactionTypeId][sourceAccountTypeId];
    }

    static AllowableSourceDestinationAccounts()
    {
        _allowableSourceDestinations = new Dictionary<Guid, IDictionary<Guid, HashSet<Guid>>>
        {
            {
                TransactionTypeConstants.Withdrawal,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Asset,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Expense,
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage,
                            AccountTypeConstants.Cash
                        }
                    },
                    {
                        AccountTypeConstants.Loan,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Expense,
                            AccountTypeConstants.Cash
                        }
                    },
                    {
                        AccountTypeConstants.Debt,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Expense,
                            AccountTypeConstants.Cash
                        }
                    },
                    {
                        AccountTypeConstants.Mortgage,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Expense,
                            AccountTypeConstants.Cash
                        }
                    }
                }
            },
            {
                TransactionTypeConstants.Deposit,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Revenue,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset,
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    },
                    {
                        AccountTypeConstants.Cash,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset,
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    },
                    {
                        AccountTypeConstants.Loan,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset
                        }
                    },
                    {
                        AccountTypeConstants.Debt,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset
                        }
                    },
                    {
                        AccountTypeConstants.Mortgage,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset
                        }
                    }
                }
            },
            {
                TransactionTypeConstants.Transfer,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Asset,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset
                        }
                    },
                    {
                        AccountTypeConstants.Loan,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    },
                    {
                        AccountTypeConstants.Debt,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    },
                    {
                        AccountTypeConstants.Mortgage,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    }
                }
            },
            {
                TransactionTypeConstants.OpeningBalance,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Asset,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.InitialBalance
                        }
                    },
                    {
                        AccountTypeConstants.Loan,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.InitialBalance
                        }
                    },
                    {
                        AccountTypeConstants.Debt,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.InitialBalance
                        }
                    },
                    {
                        AccountTypeConstants.Mortgage,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.InitialBalance
                        }
                    },
                    {
                        AccountTypeConstants.InitialBalance,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset,
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    }
                }
            },
            {
                TransactionTypeConstants.Reconciliation,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Reconciliation,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Asset
                        }
                    },
                    {
                        AccountTypeConstants.Asset,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Reconciliation
                        }
                    }
                }
            },
            {
                TransactionTypeConstants.LiabilityCredit,
                new Dictionary<Guid, HashSet<Guid>>
                {
                    {
                        AccountTypeConstants.Loan,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.LiabilityCredit
                        }
                    },
                    {
                        AccountTypeConstants.Debt,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.LiabilityCredit
                        }
                    },
                    {
                        AccountTypeConstants.Mortgage,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.LiabilityCredit
                        }
                    },
                    {
                        AccountTypeConstants.LiabilityCredit,
                        new HashSet<Guid>
                        {
                            AccountTypeConstants.Loan,
                            AccountTypeConstants.Debt,
                            AccountTypeConstants.Mortgage
                        }
                    }
                }
            }
        };
    }

}