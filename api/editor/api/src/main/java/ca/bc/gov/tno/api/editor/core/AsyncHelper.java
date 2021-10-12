package ca.bc.gov.tno.api.editor.core;

import java.util.concurrent.Future;

/**
 * Provides helper functions for async actions.
 */
public final class AsyncHelper {

  /**
   * Wait until the specified 'future' is done.
   * 
   * @param <T>    The type of future
   * @param future The future to wait for
   * @throws InterruptedException
   */
  public static <T> Future<T> wait(Future<T> future) throws InterruptedException {
    return wait(future, 100);
  }

  /**
   * Wait until the specified 'future' is done.
   * 
   * @param <T>               The type of future
   * @param future            The future to wait for
   * @param sleepMilliseconds The thread sleep length of time between checks to
   *                          determine if future is done
   * @throws InterruptedException
   */
  public static <T> Future<T> wait(Future<T> future, int sleepMilliseconds) throws InterruptedException {
    while (!future.isDone()) {
      // Wait until done.
      Thread.sleep(sleepMilliseconds);
    }

    return future;
  }
}
