using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCounter.Repository;

public interface IBlobStorageService
{
    Task<string> SaveFileAsync(string fileName, string content);
    Task<string> ReadFileAsync(string fileName);
}
