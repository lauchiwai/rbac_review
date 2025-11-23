using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models;

public class Reviews
{
    public int ReviewId { get; set; }

    public int TodoId { get; set; }

    public int ReviewerRole { get; set; }

    public int ReviewLevel { get; set; }

    public string Action { get; set; } = null!;

    public DateTime ReviewedAt { get; set; }

    public virtual TodoLists Todo { get; set; } = null!;

    public virtual Roles ReviewerRoleNavigation { get; set; } = null!;
}