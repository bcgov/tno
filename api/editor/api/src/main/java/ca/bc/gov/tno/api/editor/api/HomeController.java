package ca.bc.gov.tno.api.editor.api;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HomeController {

	@GetMapping("/")
	public String index() {
		return "Hello World";
	}

	@GetMapping("/{id}")
	public String test(@PathVariable String id) {
		return id;
	}

}
