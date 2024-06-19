﻿using Infrastructure.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }

        void Commit();

        void RejectChanges();

        void Dispose();
    }
}
