package ca.bc.gov.tno.dal.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.entities.User;

@Repository
public interface IUserRepository extends CrudRepository<User, Long> {

}
