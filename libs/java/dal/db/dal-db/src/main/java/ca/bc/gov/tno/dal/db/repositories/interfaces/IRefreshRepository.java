package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.repository.NoRepositoryBean;

import java.io.Serializable;

/**
 * IRefreshRepository interface, provides a repository with a refresh feature.
 */
@NoRepositoryBean
public interface IRefreshRepository<T, ID extends Serializable> extends JpaRepository<T, ID> {
  /**
   * Refresh the specified entity.
   * 
   * @param t The entity to refresh.
   */
  void refresh(T t);
}