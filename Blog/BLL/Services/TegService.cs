using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TegService : ITegService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TegService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        
        public IEnumerable<TegDto> GetAllTegs()
        {
            return _mapper.Map<IEnumerable<TegDto>>(_unitOfWork.TegRepository.Get());
        }

        public async Task<TegDto> GetTegById(int id)
        {
            var teg = await _unitOfWork.TegRepository.GetByIdAsync(id);
            if (teg == null) throw new ArgumentNullException(nameof(teg));
            return _mapper.Map<TegDto>(teg);
        }
    }
}
