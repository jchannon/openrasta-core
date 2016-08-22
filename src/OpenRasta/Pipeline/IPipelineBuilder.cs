﻿using System;
using System.Threading.Tasks;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public interface IPipelineBuilder
  {
    IPipelineExecutionOrder Use(Func<ICommunicationContext, Task<PipelineContinuation>> action);
  }
}
