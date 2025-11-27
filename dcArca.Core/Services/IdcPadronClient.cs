/*
 * Copyright (c) 2025 Diego Cofré, DC Sistemas
 * www.diegocofre.com.ar
 *
 * Licensed under the Apache License, Version 2.0.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 */

using dcArca.Core.Models;

namespace dcArca.Core.Services;

public interface IdcPadronClient
{
    Task<dcPadronPersonaResult> GetPersonaAsync(long cuit, CancellationToken cancellationToken = default);
}
