class ReqresRoutes {
  constructor(apiClient) {
    this.client = apiClient;
  }

  // List users
  async listUsers(page = 1) {
    return await this.client.get(`/users?page=${page}`);
  }

  // Get single user
  async getUser(id) {
    return await this.client.get(`/users/${id}`);
  }

  // Create user
  async createUser(payload) {
    return await this.client.post('/users', payload);
  }

  // Update user
  async updateUser(id, payload) {
    return await this.client.put(`/users/${id}`, payload);
  }

  // Delete user
  async deleteUser(id) {
    return await this.client.delete(`/users/${id}`);
  }
}

module.exports = ReqresRoutes;