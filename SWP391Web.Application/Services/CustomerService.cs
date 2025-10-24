    using Amazon.Runtime.Internal.UserAgent;
    using AutoMapper;
    using SWP391Web.Application.DTO.Auth;
    using SWP391Web.Application.DTO.Customer;
    using SWP391Web.Application.IService;
    using SWP391Web.Application.IServices;
    using SWP391Web.Domain.Entities;
    using SWP391Web.Infrastructure.IRepository;
    using System.Security.Claims;

    namespace SWP391Web.Application.Services
    {
        public class CustomerService : ICustomerService
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            }

            public async Task<ResponseDTO> CreateCustomerAsync(ClaimsPrincipal user, CreateCustomerDTO createCustomerDTO)
            {
                try
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "User not found",
                            StatusCode = 404
                        };
                    }

                    var dealer = await _unitOfWork.DealerRepository.GetTrackedDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found",
                            StatusCode = 404,
                        };
                    }

                    var existPhone = dealer.Customers.FirstOrDefault(c => c.PhoneNumber == createCustomerDTO.PhoneNumber);
                    if (existPhone != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Customer phone number already exists in this dealer.",
                            StatusCode = 400,
                        };
                    }

                    var customer = new Customer
                    {
                        FullName = createCustomerDTO.FullName,
                        PhoneNumber = createCustomerDTO.PhoneNumber,
                        Address = createCustomerDTO.Address,
                        CreatedAt = createCustomerDTO.CreatedAt,
                        Note = createCustomerDTO.Note
                    };

                    customer.Dealers.Add(dealer);

                    await _unitOfWork.CustomerRepository.AddAsync(customer, CancellationToken.None);
                    await _unitOfWork.SaveAsync();

                    var getCustomerDTO = _mapper.Map<GetCustomerDTO>(customer);

                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Create customer successfully.",
                        StatusCode = 200,
                        Result = getCustomerDTO
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = ex.Message,
                        StatusCode = 500
                    };
                }
            }


            public async Task<ResponseDTO> GetAllCustomerAsync(ClaimsPrincipal user)
            {
                try
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 401,
                            Message = "User not found. "
                        };
                    }
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 403,
                            Message = "Dealer not found."
                        };
                    }
                    var customers = await _unitOfWork.CustomerRepository.GetAllAsync();
                    var customerDTOs = _mapper.Map<List<GetCustomerDTO>>(customers);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Customers retrieved successfully",
                        Result = customerDTOs
                    };

                }
                catch (Exception ex)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = ex.Message
                    };
                }
            }

            public async Task<ResponseDTO> GetCustomerByIdAsync(ClaimsPrincipal user, Guid customerId)
            {
                try
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 401,
                            Message = "User not found. "
                        };
                    }

                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 403,
                            Message = "Dealer not found."
                        };
                    }

                    var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);

                    if (customer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Customer not found"
                        };
                    }
                    var customerDTO = _mapper.Map<GetCustomerDTO>(customer);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Customer retrieved successfully",
                        Result = customerDTO
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = ex.Message
                    };
                }
            }

            public async Task<ResponseDTO> UpdateCustomerAsync(ClaimsPrincipal user, Guid customerId, UpdateCustomerDTO updateCustomerDTO)
            {
                try
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 401,
                            Message = "User not found. "
                        };
                    }
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                    if (dealer == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 403,
                            Message = "Dealer not found."
                        };
                    }

                    var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                    if (customer == null)
                    {   
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Customer not found"
                        };
                    }
                    // check if customer belong to dealer
                    var belongToDealer = dealer.Customers.Any(c => c.Id == customerId);
                    if (!belongToDealer)
                        {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 403,
                            Message = "You are not allowed to update this customer."
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(updateCustomerDTO.PhoneNumber))
                    {
                        var existPhone = dealer.Customers
                            .FirstOrDefault(c => c.PhoneNumber == updateCustomerDTO.PhoneNumber && c.Id != customer.Id);

                        if (existPhone != null)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = "This phone number already exists for another customer in your dealer."
                            };
                        }

                        customer.PhoneNumber = updateCustomerDTO.PhoneNumber.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(updateCustomerDTO.FullName))
                    {
                        customer.FullName = updateCustomerDTO.FullName;
                    }

                    if (!string.IsNullOrWhiteSpace(updateCustomerDTO.Address))
                    {
                        customer.Address = updateCustomerDTO.Address;
                    }

                    if (!string.IsNullOrWhiteSpace(updateCustomerDTO.Note))
                    {
                        customer.Note = updateCustomerDTO.Note;
                    }

                    _unitOfWork.CustomerRepository.Update(customer);
                    await _unitOfWork.SaveAsync();

                    var getCustomer = _mapper.Map<GetCustomerDTO>(customer);

                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "Update customer successfully.",
                        Result = getCustomer
                    };

                }
                catch (Exception ex)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = ex.Message
                    };

                }            
            
            }
        }
    }
