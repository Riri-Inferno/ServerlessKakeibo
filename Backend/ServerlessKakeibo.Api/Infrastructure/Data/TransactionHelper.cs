using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Data;

/// <summary>
/// トランザクション管理ヘルパー
/// </summary>
public class TransactionHelper : ITransactionHelper
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionHelper> _logger;

    public TransactionHelper(
        ApplicationDbContext context,
        ILogger<TransactionHelper> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// トランザクション付きで処理を実行
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <exception cref="InvalidOperationException">同時実行の競合が発生した場合</exception>
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Concurrency conflict occurred during transaction execution");
                throw new InvalidOperationException("データが他のユーザーによって更新されています。再度お試しください。", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction execution failed");
                throw new InvalidOperationException("トランザクションの実行中にエラーが発生しました。", ex);
            }
        });
    }

    /// <summary>
    /// トランザクション付きで処理を実行し、結果を返す
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する関数</param>
    /// <returns>関数の実行結果</returns>
    /// <exception cref="InvalidOperationException">同時実行の競合またはトランザクションエラーが発生した場合</exception>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await func();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Concurrency conflict occurred during transaction execution");
                throw new InvalidOperationException("データが他のユーザーによって更新されています。再度お試しください。", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction execution failed");
                throw new InvalidOperationException("トランザクションの実行中にエラーが発生しました。", ex);
            }
        });
    }
}
