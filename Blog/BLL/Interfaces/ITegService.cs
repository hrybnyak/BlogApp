using BLL.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITegService
    {
        IEnumerable<TegDto> GetAllTegs();
        Task<TegDto> GetTegById(int id);
    }
}
