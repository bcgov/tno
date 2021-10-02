package ca.bc.gov.tno.api.editor.api.controllers;

import org.elasticsearch.client.core.MainResponse;

import java.io.IOException;

import org.elasticsearch.client.RequestOptions;
import org.elasticsearch.client.RestHighLevelClient;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/elastic")
public class ElasticController {

	@Autowired
	RestHighLevelClient elasticClient;

	@GetMapping("")
	public MainResponse index() throws IOException {

		MainResponse response = elasticClient.info(RequestOptions.DEFAULT);
		return response;
	}

}
