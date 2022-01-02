using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _5GuysLMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _5GuysLMS.Data.Configurations
{
    public class ProfilePictureConfiguration : IEntityTypeConfiguration<ProfilePicture>
    {
        public void Configure(EntityTypeBuilder<ProfilePicture> builder)
        {

        }
    }
}
