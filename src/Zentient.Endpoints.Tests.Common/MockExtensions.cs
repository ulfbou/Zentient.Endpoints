// <copyright file="MockExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    static class MockExtensions
    {
        public static void ApplyResultProperties<TMock, TResult>(this Mock<TMock> mock, TResult result)
          where TMock : class
          where TResult : IResult
        {
            mock.SetupGet(m => ((IResult)m).IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet(m => ((IResult)m).IsFailure).Returns(result.IsFailure);
            mock.SetupGet(m => ((IResult)m).Errors).Returns(result.Errors);
            mock.SetupGet(m => ((IResult)m).Messages).Returns(result.Messages);
            mock.SetupGet(m => ((IResult)m).ErrorMessage).Returns(result.ErrorMessage);
            mock.SetupGet(m => ((IResult)m).Status).Returns(result.Status);
        }
    }
}
