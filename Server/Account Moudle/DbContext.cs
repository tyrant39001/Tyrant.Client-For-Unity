using Great_Wisdom_Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Tyrant.EntityFrameworkCore.Common.Extension.DataAnnotations;

namespace Account_Module
{
    public partial class AccountInfoAssist
    {
        public class DbContext : AccountDbContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder); // 一定要调用基类方法
            }
        }
    }
}