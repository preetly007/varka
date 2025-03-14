using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Varca.Domain.Models;

public class VarcaId
{
    public static string New() {
        return Ulid.NewUlid().ToString();
    }

    public static bool TryCheck(string varcaId) {
        return Ulid.TryParse(varcaId, out Ulid ulid);
    }
}
