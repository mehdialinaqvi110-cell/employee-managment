
const API_URL = "http://localhost:5164/api/Employee";

async function handleResponse(response) {
  if (!response.ok) {
    let message = `Request failed with status ${response.status}`;

    try {
      const data = await response.json();
      message = data.message || data.title || message;
    } catch {
      // Keep the default message if the response has no JSON body.
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return await response.json();
}

export async function getEmployees() {
  const response = await fetch(API_URL);
  return await handleResponse(response);
}

export async function createEmployee(employee) {
  const response = await fetch(API_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(employee)
  });

  return await handleResponse(response);
}

export async function updateEmployee(id, employee) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(employee)
  });

  return await handleResponse(response);
}

export async function deleteEmployee(id) {
  const response = await fetch(`${API_URL}/${id}`, {
    method: "DELETE"
  });

  return await handleResponse(response);
}
