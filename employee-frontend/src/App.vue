<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import {
  getEmployees,
  createEmployee,
  updateEmployee,
  deleteEmployee
} from './services/employeeServices';

const employees = ref([]);
const searchQuery = ref('');
const selectedDepartment = ref('');
const activeSection = ref('Dashboard');

const departments = computed(() => {
  return [
    ...new Set(
      employees.value
        .map(employee => employee.department)
        .filter(Boolean)
    )
  ].sort();
});

const filteredEmployees = computed(() => {
  const query = searchQuery.value.trim().toLowerCase();

  return employees.value.filter(employee => {
    const fullName = String(employee.fullName || '').toLowerCase();
    const email = String(employee.email || '').toLowerCase();

    const matchesSearch =
      fullName.includes(query) || email.includes(query);

    const matchesDepartment =
      !selectedDepartment.value ||
      employee.department === selectedDepartment.value;

    return matchesSearch && matchesDepartment;
  });
});

// Dashboard statistics
const totalEmployees = computed(() => employees.value.length);
const totalDepartments = computed(() => departments.value.length);

const averageSalary = computed(() => {
  if (employees.value.length === 0) return 0;

  const total = employees.value.reduce(
    (sum, employee) => sum + Number(employee.salary || 0),
    0
  );

  return total / employees.value.length;
});

const highestSalary = computed(() => {
  if (employees.value.length === 0) return 0;

  return Math.max(
    ...employees.value.map(employee => Number(employee.salary || 0))
  );
});

const departmentSummary = computed(() => {
  return departments.value.map(department => ({
    name: department,
    count: employees.value.filter(
      employee => employee.department === department
    ).length
  }));
});

// Pagination
const currentPage = ref(1);
const pageSize = ref(5);

const totalPages = computed(() => {
  return Math.ceil(filteredEmployees.value.length / pageSize.value);
});

const paginatedEmployees = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredEmployees.value.slice(start, start + pageSize.value);
});

const startPage = computed(() => {
  return filteredEmployees.value.length === 0
    ? 0
    : (currentPage.value - 1) * pageSize.value + 1;
});

const endPage = computed(() => {
  return Math.min(
    currentPage.value * pageSize.value,
    filteredEmployees.value.length
  );
});

function changePage(page) {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
  }
}

function changePageSize(event) {
  pageSize.value = Number(event.target.value);
  currentPage.value = 1;
}

watch([searchQuery, selectedDepartment, pageSize], () => {
  currentPage.value = 1;
});

watch(totalPages, newTotalPages => {
  if (newTotalPages === 0) {
    currentPage.value = 1;
  } else if (currentPage.value > newTotalPages) {
    currentPage.value = newTotalPages;
  }
});

// Form and messages
const errorMessage = ref('');
const successMessage = ref('');
const isSubmitting = ref(false);
const editingId = ref(null);
const employeeToDelete = ref(null);
const isDeleting = ref(false);

const emptyForm = () => ({
  fullName: '',
  email: '',
  department: '',
  salary: ''
});

const form = ref(emptyForm());

async function loadEmployees() {
  try {
    const data = await getEmployees();
    employees.value = Array.isArray(data) ? data : [];
    errorMessage.value = '';
  } catch (error) {
    errorMessage.value = 'Could not load employees. Check the backend.';
    console.error(error);
  }
}

function clearMessages() {
  errorMessage.value = '';
  successMessage.value = '';
}

function resetForm() {
  form.value = emptyForm();
  editingId.value = null;
}

function openEmployees() {
  activeSection.value = 'Employees';
}

function startAddingEmployee() {
  clearMessages();
  resetForm();
  activeSection.value = 'Employees';
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function editEmployee(employee) {
  clearMessages();
  activeSection.value = 'Employees';
  editingId.value = employee.id;

  form.value = {
    fullName: employee.fullName ?? '',
    email: employee.email ?? '',
    department: employee.department ?? '',
    salary: employee.salary ?? ''
  };

  window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function saveEmployee() {
  clearMessages();
  isSubmitting.value = true;

  try {
    const employee = {
      fullName: form.value.fullName.trim(),
      email: form.value.email.trim(),
      department: form.value.department.trim(),
      salary: Number(form.value.salary)
    };

    if (editingId.value !== null) {
      await updateEmployee(editingId.value, {
        ...employee,
        id: editingId.value
      });
      successMessage.value = 'Employee updated successfully!';
    } else {
      await createEmployee(employee);
      successMessage.value = 'Employee added successfully!';
    }

    resetForm();
    await loadEmployees();
  } catch (error) {
    errorMessage.value =
      error.message || 'Operation failed. Please check the backend.';
    console.error(error);
  } finally {
    isSubmitting.value = false;
  }
}

function askToDelete(employee) {
  employeeToDelete.value = employee;
}

function cancelDelete() {
  if (isDeleting.value) return;
  employeeToDelete.value = null;
}

async function confirmDelete() {
  if (!employeeToDelete.value || isDeleting.value) return;

  const employee = employeeToDelete.value;
  isDeleting.value = true;
  clearMessages();

  try {
    await deleteEmployee(employee.id);

    if (editingId.value === employee.id) {
      resetForm();
    }

    employeeToDelete.value = null;
    successMessage.value = `${employee.fullName} was deleted successfully.`;
    await loadEmployees();
  } catch (error) {
    errorMessage.value = error.message || 'Failed to delete employee.';
    console.error(error);
  } finally {
    isDeleting.value = false;
  }
}

onMounted(loadEmployees);
</script>

<template>
  <div class="app-layout">
    <aside class="sidebar">
      <div class="brand">
        <div class="brand-mark">E</div>
        <div>
          <strong>EasyEmployee</strong>
          <small>Management System</small>
        </div>
      </div>

      <div class="sidebar-label">MAIN MENU</div>
      <nav class="side-nav" aria-label="Main navigation">
        <button
          type="button"
          class="nav-item"
          :class="{ active: activeSection === 'Dashboard' }"
          @click="activeSection = 'Dashboard'"
        >
          <span class="nav-icon">▦</span> Dashboard
        </button>
        <button
          type="button"
          class="nav-item"
          :class="{ active: activeSection === 'Employees' }"
          @click="openEmployees"
        >
          <span class="nav-icon">♙</span> Employees
        </button>
        <button
          type="button"
          class="nav-item"
          :class="{ active: activeSection === 'Reports' }"
          @click="activeSection = 'Reports'"
        >
          <span class="nav-icon">▤</span> Reports
        </button>
      </nav>

      <div class="sidebar-bottom">
        <div class="help-box">
          <strong>Need a hand?</strong>
          <p>Manage your employee records from one place.</p>
        </div>
        <div class="profile">
          <div class="avatar">MA</div>
          <div class="profile-text">
            <strong>Admin User</strong>
            <small>Administrator</small>
          </div>
        </div>
      </div>
    </aside>

    <main class="main-area">
      <header class="topbar">
        <div>
          <p class="eyebrow">EMPLOYEE MANAGEMENT</p>
          <h1>{{ activeSection }}</h1>
        </div>
        <div class="topbar-actions">
          <span class="today-label">Simple team overview</span>
          <button type="button" class="primary-button" @click="startAddingEmployee">
            <span>＋</span> Add Employee
          </button>
        </div>
      </header>

      <div v-if="successMessage" class="message success">{{ successMessage }}</div>
      <div v-if="errorMessage" class="message error">{{ errorMessage }}</div>

      <template v-if="activeSection === 'Dashboard'">
        <section class="welcome-row">
          <div>
            <h2>Welcome back!</h2>
            <p>Here is a quick look at your employee records.</p>
          </div>
          <span class="welcome-icon">✦</span>
        </section>

        <section class="dashboard">
          <article class="stat-card">
            <div class="stat-top">
              <span class="stat-icon purple">♙</span>
              <span class="stat-label">All employees</span>
            </div>
            <p class="stat-value">{{ totalEmployees }}</p>
            <p class="stat-note">Employee records in the system</p>
          </article>

          <article class="stat-card">
            <div class="stat-top">
              <span class="stat-icon teal">▦</span>
              <span class="stat-label">Departments</span>
            </div>
            <p class="stat-value">{{ totalDepartments }}</p>
            <p class="stat-note">Different departments represented</p>
          </article>

          <article class="stat-card">
            <div class="stat-top">
              <span class="stat-icon orange">₹</span>
              <span class="stat-label">Average salary</span>
            </div>
            <p class="stat-value salary-value">
              ₹{{ averageSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 }) }}
            </p>
            <p class="stat-note">Average across all employees</p>
          </article>

          <article class="stat-card">
            <div class="stat-top">
              <span class="stat-icon blue">↑</span>
              <span class="stat-label">Highest salary</span>
            </div>
            <p class="stat-value salary-value">
              ₹{{ highestSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 }) }}
            </p>
            <p class="stat-note">Highest recorded salary</p>
          </article>
        </section>

        <section class="content-grid">
          <article class="panel recent-panel">
            <div class="panel-heading">
              <div>
                <h2>Employee directory</h2>
                <p>A quick view of the people in your system.</p>
              </div>
              <button type="button" class="text-button" @click="openEmployees">
                View all →
              </button>
            </div>

            <div v-if="employees.length === 0" class="empty-state">
              <div class="empty-icon">♙</div>
              <h3>No employees added yet</h3>
              <p>Add your first employee to get started.</p>
              <button type="button" class="primary-button" @click="startAddingEmployee">
                Add first employee
              </button>
            </div>

            <div v-else class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>EMPLOYEE</th>
                    <th>DEPARTMENT</th>
                    <th>SALARY</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="employee in employees.slice(0, 4)" :key="employee.id">
                    <td>
                      <div class="employee-cell">
                        <div class="mini-avatar">{{ String(employee.fullName || 'E').charAt(0).toUpperCase() }}</div>
                        <div>
                          <strong>{{ employee.fullName }}</strong>
                          <small>{{ employee.email }}</small>
                        </div>
                      </div>
                    </td>
                    <td><span class="department-tag">{{ employee.department }}</span></td>
                    <td>₹{{ Number(employee.salary || 0).toLocaleString('en-IN') }}</td>
                    <td>
                      <button type="button" class="small-action" @click="editEmployee(employee)" aria-label="Edit employee">⋯</button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </article>

          <article class="panel department-panel">
            <div class="panel-heading">
              <div>
                <h2>Departments</h2>
                <p>Employees by department</p>
              </div>
            </div>

            <div v-if="departmentSummary.length === 0" class="simple-empty">
              Department information will appear here after you add employees.
            </div>

            <div v-else class="department-list">
              <div v-for="item in departmentSummary" :key="item.name" class="department-item">
                <div class="department-row">
                  <span>{{ item.name }}</span>
                  <strong>{{ item.count }}</strong>
                </div>
                <div class="progress-track">
                  <div
                    class="progress-fill"
                    :style="{ width: (totalEmployees ? item.count / totalEmployees * 100 : 0) + '%' }"
                  ></div>
                </div>
              </div>
            </div>

            <button type="button" class="outline-button full-width" @click="activeSection = 'Reports'">
              View simple report
            </button>
          </article>
        </section>
      </template>

      <template v-else-if="activeSection === 'Employees'">
        <section class="panel form-panel">
          <div class="panel-heading">
            <div>
              <h2>{{ editingId !== null ? 'Edit employee' : 'Add a new employee' }}</h2>
              <p>Enter the employee details below. Fields marked required must be completed.</p>
            </div>
          </div>

          <form class="employee-form" @submit.prevent="saveEmployee">
            <div class="form-field">
              <label for="fullName">Full name *</label>
              <input id="fullName" v-model="form.fullName" required maxlength="150" placeholder="e.g. Rahul Sharma" />
            </div>

            <div class="form-field">
              <label for="email">Email address *</label>
              <input id="email" v-model="form.email" type="email" required maxlength="254" placeholder="e.g. rahul@example.com" />
            </div>

            <div class="form-field">
              <label for="department">Department *</label>
              <input id="department" v-model="form.department" required maxlength="100" placeholder="e.g. Finance" />
            </div>

            <div class="form-field">
              <label for="salary">Salary (₹) *</label>
              <input id="salary" v-model="form.salary" type="number" min="0" step="0.01" required placeholder="e.g. 45000" />
            </div>

            <div class="form-actions">
              <button type="submit" class="primary-button" :disabled="isSubmitting">
                {{ isSubmitting ? 'Saving...' : editingId !== null ? 'Save changes' : 'Add employee' }}
              </button>
              <button v-if="editingId !== null" type="button" class="outline-button" @click="resetForm">
                Cancel edit
              </button>
            </div>
          </form>
        </section>

        <section class="panel">
          <div class="panel-heading employee-list-heading">
            <div>
              <h2>All employees</h2>
              <p>{{ filteredEmployees.length }} employee record(s) found</p>
            </div>
          </div>

          <div class="filters">
            <div class="search-box">
              <span>⌕</span>
              <input v-model="searchQuery" type="text" placeholder="Search name or email..." aria-label="Search employees by name or email" />
            </div>
            <select v-model="selectedDepartment" aria-label="Filter by department">
              <option value="">All departments</option>
              <option v-for="department in departments" :key="department" :value="department">
                {{ department }}
              </option>
            </select>
          </div>

          <div v-if="filteredEmployees.length === 0" class="empty-state compact-empty">
            <div class="empty-icon">⌕</div>
            <h3>No matching employees</h3>
            <p>Try a different search or add a new employee.</p>
          </div>

          <div v-else class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>EMPLOYEE</th>
                  <th>DEPARTMENT</th>
                  <th>SALARY</th>
                  <th class="actions-heading">ACTIONS</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="employee in paginatedEmployees" :key="employee.id">
                  <td>#{{ employee.id }}</td>
                  <td>
                    <div class="employee-cell">
                      <div class="mini-avatar">{{ String(employee.fullName || 'E').charAt(0).toUpperCase() }}</div>
                      <div>
                        <strong>{{ employee.fullName }}</strong>
                        <small>{{ employee.email }}</small>
                      </div>
                    </div>
                  </td>
                  <td><span class="department-tag">{{ employee.department }}</span></td>
                  <td>₹{{ Number(employee.salary || 0).toLocaleString('en-IN') }}</td>
                  <td class="actions">
                    <button type="button" class="edit-button" @click="editEmployee(employee)">Edit</button>
                    <button type="button" class="delete-button" @click="askToDelete(employee)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-if="filteredEmployees.length > 0" class="pagination">
            <p>Showing {{ startPage }}–{{ endPage }} of {{ filteredEmployees.length }} employees</p>
            <div class="pagination-controls">
              <label for="pageSize">Rows:</label>
              <select id="pageSize" :value="pageSize" @change="changePageSize">
                <option :value="5">5</option>
                <option :value="10">10</option>
                <option :value="20">20</option>
              </select>
              <button type="button" class="page-button" :disabled="currentPage === 1" @click="changePage(currentPage - 1)">Previous</button>
              <span>Page {{ currentPage }} of {{ totalPages }}</span>
              <button type="button" class="page-button" :disabled="currentPage >= totalPages" @click="changePage(currentPage + 1)">Next</button>
            </div>
          </div>
        </section>
      </template>

      <template v-else-if="activeSection === 'Reports'">
        <section class="welcome-row">
          <div>
            <h2>Simple employee report</h2>
            <p>A basic summary created from the records saved in your database.</p>
          </div>
          <span class="welcome-icon">▤</span>
        </section>

        <section class="dashboard report-stats">
          <article class="stat-card">
            <div class="stat-top"><span class="stat-icon purple">♙</span><span class="stat-label">Employees</span></div>
            <p class="stat-value">{{ totalEmployees }}</p>
            <p class="stat-note">Total records</p>
          </article>
          <article class="stat-card">
            <div class="stat-top"><span class="stat-icon teal">▦</span><span class="stat-label">Departments</span></div>
            <p class="stat-value">{{ totalDepartments }}</p>
            <p class="stat-note">Departments in use</p>
          </article>
          <article class="stat-card">
            <div class="stat-top"><span class="stat-icon orange">₹</span><span class="stat-label">Average salary</span></div>
            <p class="stat-value salary-value">₹{{ averageSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 }) }}</p>
            <p class="stat-note">Across all employees</p>
          </article>
          <article class="stat-card">
            <div class="stat-top"><span class="stat-icon blue">↑</span><span class="stat-label">Highest salary</span></div>
            <p class="stat-value salary-value">₹{{ highestSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 }) }}</p>
            <p class="stat-note">Highest recorded salary</p>
          </article>
        </section>

        <section class="panel">
          <div class="panel-heading">
            <div>
              <h2>Department breakdown</h2>
              <p>Number of employees in each department</p>
            </div>
          </div>
          <div v-if="departmentSummary.length === 0" class="simple-empty">
            No department data yet. Add employees to build this report.
          </div>
          <div v-else class="department-list report-department-list">
            <div v-for="item in departmentSummary" :key="item.name" class="department-item">
              <div class="department-row">
                <span>{{ item.name }}</span>
                <strong>{{ item.count }} employee(s)</strong>
              </div>
              <div class="progress-track">
                <div class="progress-fill" :style="{ width: (totalEmployees ? item.count / totalEmployees * 100 : 0) + '%' }"></div>
              </div>
            </div>
          </div>
        </section>
      </template>

      <footer class="footer">
        <span>EasyEmployee · Student project</span>
        <span>Built with Vue.js, ASP.NET Core and MySQL</span>
      </footer>
    </main>

    <!-- Custom delete confirmation dialog -->
    <div
      v-if="employeeToDelete"
      class="modal-backdrop"
      @click.self="cancelDelete"
      @keydown.esc="cancelDelete"
    >
      <section
        class="confirm-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
      >
        <div class="modal-warning-icon">!</div>
        <h2 id="delete-dialog-title">Delete employee?</h2>
        <p id="delete-dialog-description">
          Are you sure you want to delete
          <strong>{{ employeeToDelete.fullName }}</strong>?
          This action cannot be undone.
        </p>
        <div class="modal-actions">
          <button
            type="button"
            class="outline-button"
            :disabled="isDeleting"
            @click="cancelDelete"
          >
            Cancel
          </button>
          <button
            type="button"
            class="modal-delete-button"
            :disabled="isDeleting"
            @click="confirmDelete"
          >
            {{ isDeleting ? 'Deleting...' : 'Yes, delete employee' }}
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<style>
:root {
  font-family: Inter, "Segoe UI", Arial, sans-serif;
  color: #263247;
  background: #f4f6fb;
  font-synthesis: none;
}
* { box-sizing: border-box; }
body { margin: 0; min-width: 320px; background: #f4f6fb; }
button, input, select { font: inherit; }
button { cursor: pointer; }
button:disabled { cursor: not-allowed; opacity: .5; }

/* Override the default Vite starter #app sizing so the dashboard can use the full screen. */
html, body, #app {
  width: 100%;
  min-width: 320px;
  min-height: 100%;
  margin: 0;
  padding: 0;
  overflow-x: hidden;
}
#app {
  max-width: none;
  text-align: left;
}
.app-layout {
  display: block;
  width: 100%;
  min-height: 100vh;
}
.sidebar {
  width: 238px;
  position: fixed;
  inset: 0 auto 0 0;
  background: #fff;
  border-right: 1px solid #e8ecf3;
  display: flex;
  flex-direction: column;
  padding: 26px 16px 18px;
  z-index: 5;
}
.brand { display: flex; align-items: center; gap: 11px; padding: 0 8px 34px; }
.brand-mark {
  width: 38px; height: 38px; border-radius: 12px; display: grid; place-items: center;
  color: white; background: #6257d9; font-size: 20px; font-weight: 700;
}
.brand strong { display: block; font-size: 15px; letter-spacing: -.3px; }
.brand small { display: block; color: #929bad; font-size: 11px; margin-top: 3px; }
.sidebar-label { color: #a1a9b8; font-size: 10px; letter-spacing: 1.3px; font-weight: 700; padding: 0 12px 12px; }
.side-nav { display: grid; gap: 6px; }
.nav-item {
  display: flex; align-items: center; gap: 12px; width: 100%; text-align: left;
  padding: 12px 13px; color: #667187; border: 0; border-radius: 9px; background: transparent;
  font-size: 13px; transition: background .15s, color .15s;
}
.nav-item:hover { background: #f5f4ff; color: #5147c8; }
.nav-item.active { color: #5147c8; background: #efedff; font-weight: 600; }
.nav-icon { display: inline-grid; place-items: center; width: 20px; font-size: 18px; }
.sidebar-bottom { margin-top: auto; }
.help-box { margin: 0 4px 18px; padding: 15px; border-radius: 11px; background: #f6f7fc; }
.help-box strong { font-size: 12px; }
.help-box p { color: #8791a4; font-size: 11px; line-height: 1.5; margin: 7px 0 0; }
.profile { border-top: 1px solid #edf0f5; padding: 18px 6px 0; display: flex; align-items: center; gap: 10px; }
.avatar { width: 36px; height: 36px; border-radius: 50%; display: grid; place-items: center; background: #e3e0ff; color: #5147c8; font-size: 12px; font-weight: 700; }
.profile-text strong { display: block; font-size: 12px; }
.profile-text small { display: block; color: #929bad; font-size: 10px; margin-top: 4px; }

.main-area {
  box-sizing: border-box;
  margin-left: 238px;
  width: calc(100% - 238px);
  max-width: none;
  min-width: 0;
  padding: 30px 34px 20px;
}
.topbar { display: flex; justify-content: space-between; align-items: center; gap: 18px; margin-bottom: 28px; }
.eyebrow { margin: 0 0 6px; color: #8a94a7; font-size: 10px; letter-spacing: 1.3px; font-weight: 700; }
h1 { margin: 0; font-size: 26px; font-weight: 650; letter-spacing: -.7px; color: #202b40; }
.topbar-actions { display: flex; align-items: center; gap: 18px; }
.today-label { color: #8791a4; font-size: 12px; }
.primary-button {
  border: 0; background: #6257d9; color: white; border-radius: 8px; padding: 11px 15px;
  font-size: 12px; font-weight: 600; box-shadow: 0 3px 8px #6257d91f;
}
.primary-button:hover { background: #5147c8; }
.primary-button span { font-size: 17px; vertical-align: -1px; margin-right: 4px; }
.message { padding: 12px 15px; border-radius: 8px; margin-bottom: 16px; font-size: 13px; }
.success { color: #16794b; background: #eaf8f0; border: 1px solid #c9eed9; }
.error { color: #a72b36; background: #fff0f1; border: 1px solid #ffd5d8; }
.welcome-row { display: flex; justify-content: space-between; align-items: center; margin: 0 0 18px; }
.welcome-row h2 { margin: 0 0 6px; color: #263247; font-size: 17px; }
.welcome-row p { margin: 0; color: #8791a4; font-size: 12px; }
.welcome-icon { display: grid; place-items: center; width: 38px; height: 38px; border-radius: 12px; background: #eae8ff; color: #6257d9; font-size: 20px; }

.dashboard {
  display: grid;
  /* Cards stay readable instead of becoming too narrow on smaller screens. */
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 210px), 1fr));
  gap: 16px;
  margin-bottom: 20px;
}
.stat-card, .panel { background: #fff; border: 1px solid #edf0f5; border-radius: 12px; box-shadow: 0 3px 12px #23304b05; }
.stat-card {
  padding: 18px;
  min-width: 0;
  overflow: hidden;
}
.stat-top { display: flex; align-items: center; gap: 9px; }
.stat-icon { width: 30px; height: 30px; display: grid; place-items: center; border-radius: 9px; font-size: 15px; font-weight: 600; }
.purple { background: #efedff; color: #7164df; }
.teal { background: #e2f8f6; color: #1baea9; }
.orange { background: #fff1e6; color: #e48c48; }
.blue { background: #e9f1ff; color: #4d85e8; }
.stat-label { color: #7c879a; font-size: 11px; }
.stat-value {
  margin: 17px 0 7px;
  color: #263247;
  font-size: 27px;
  font-weight: 650;
  letter-spacing: -.7px;
  overflow-wrap: anywhere;
}
.salary-value {
  font-size: clamp(15px, 1.5vw, 22px);
  line-height: 1.25;
  letter-spacing: -.4px;
}
.stat-note { margin: 0; color: #9aa3b2; font-size: 10px; line-height: 1.5; }
.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.5fr) minmax(260px, 1fr);
  gap: 18px;
  align-items: start;
}
.content-grid > .panel { min-width: 0; }
.recent-panel .panel-heading { flex-wrap: wrap; }
.recent-panel .panel-heading > div { min-width: 0; }
.panel { padding: 21px; margin-bottom: 18px; min-width: 0; }
.panel-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 14px; margin-bottom: 18px; }
.panel-heading h2 { font-size: 15px; margin: 0 0 6px; color: #29354a; }
.panel-heading p { color: #929bad; font-size: 11px; margin: 0; line-height: 1.5; }
.text-button { background: transparent; border: 0; color: #6257d9; font-size: 11px; font-weight: 600; white-space: nowrap; padding: 5px; }
.table-wrapper { width: 100%; overflow-x: auto; }
table { width: 100%; border-collapse: collapse; text-align: left; }
th { color: #9aa3b2; font-size: 9px; letter-spacing: .6px; font-weight: 700; padding: 11px 9px; border-bottom: 1px solid #edf0f5; white-space: nowrap; }
td { padding: 12px 9px; border-bottom: 1px solid #f0f2f6; font-size: 11px; color: #536075; white-space: nowrap; }
tbody tr:last-child td { border-bottom: 0; }
.employee-cell { display: flex; align-items: center; gap: 9px; min-width: 160px; }
.mini-avatar { flex: 0 0 30px; width: 30px; height: 30px; border-radius: 50%; background: #eeeaff; color: #6257d9; display: grid; place-items: center; font-size: 11px; font-weight: 700; }
.employee-cell strong { display: block; color: #344057; font-size: 11px; font-weight: 600; }
.employee-cell small { display: block; color: #9aa3b2; font-size: 9px; margin-top: 4px; }
.department-tag { display: inline-block; padding: 5px 8px; border-radius: 6px; background: #f1f3f8; color: #657087; font-size: 10px; }
.small-action { background: transparent; border: 0; color: #8791a4; font-size: 19px; padding: 0 6px; }
.department-list { display: grid; gap: 19px; margin: 5px 0 22px; }
.department-row { display: flex; justify-content: space-between; gap: 10px; color: #667187; font-size: 11px; margin-bottom: 8px; }
.department-row strong { color: #344057; font-size: 11px; }
.progress-track { height: 6px; border-radius: 10px; background: #f0f1f7; overflow: hidden; }
.progress-fill { height: 100%; border-radius: inherit; background: #766ae4; }
.outline-button { background: #fff; border: 1px solid #dfe3ec; color: #58647a; border-radius: 8px; padding: 10px 13px; font-size: 11px; font-weight: 600; }
.outline-button:hover { border-color: #b8b1ff; color: #5147c8; }
.full-width { width: 100%; }
.simple-empty { color: #9aa3b2; font-size: 11px; line-height: 1.6; padding: 14px 0 22px; }
.empty-state { text-align: center; padding: 34px 15px; }
.empty-icon { width: 44px; height: 44px; margin: 0 auto 12px; border-radius: 14px; display: grid; place-items: center; background: #f0eeff; color: #6257d9; font-size: 22px; }
.empty-state h3 { margin: 0 0 7px; font-size: 14px; }
.empty-state p { margin: 0 0 16px; color: #929bad; font-size: 11px; }
.compact-empty { padding: 30px 10px; }

.form-panel { max-width: 100%; }
.employee-form { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 18px; }
.form-field { display: grid; gap: 7px; }
.form-field label { font-size: 11px; color: #5e6a80; font-weight: 600; }
.form-field input, .filters input, .filters select, .pagination select {
  width: 100%; min-width: 0; border: 1px solid #e0e5ee; border-radius: 7px; background: #fff;
  color: #344057; padding: 11px 12px; font-size: 12px; outline: none;
}
.form-field input:focus, .filters input:focus, .filters select:focus, .pagination select:focus { border-color: #9d95f0; box-shadow: 0 0 0 3px #6257d910; }
.form-field input::placeholder, .filters input::placeholder { color: #b0b7c4; }
.form-actions { grid-column: 1 / -1; display: flex; gap: 10px; padding-top: 2px; }
.filters { display: flex; gap: 12px; margin-bottom: 15px; }
.search-box { flex: 1; display: flex; align-items: center; gap: 7px; border: 1px solid #e0e5ee; border-radius: 7px; padding-left: 11px; color: #929bad; }
.search-box span { font-size: 21px; }
.search-box input { border: 0; box-shadow: none !important; }
.filters select { width: 190px; }
.actions { display: flex; gap: 6px; }
.edit-button, .delete-button { border-radius: 6px; padding: 7px 9px; font-size: 10px; border: 1px solid transparent; }
.edit-button { color: #5147c8; background: #efedff; }
.delete-button { color: #c34a55; background: #fff0f1; }
.actions-heading { text-align: right; }
.pagination { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 14px; border-top: 1px solid #edf0f5; padding-top: 17px; margin-top: 8px; }
.pagination > p { margin: 0; color: #929bad; font-size: 10px; }
.pagination-controls { display: flex; align-items: center; flex-wrap: wrap; gap: 9px; color: #7c879a; font-size: 10px; }
.pagination select { width: 65px; padding: 7px; font-size: 10px; }
.page-button { border: 1px solid #e0e5ee; border-radius: 6px; background: #fff; color: #58647a; padding: 8px 10px; font-size: 10px; }
.page-button:disabled { opacity: .45; }
.footer { display: flex; justify-content: space-between; gap: 12px; color: #a2aaba; font-size: 10px; padding: 8px 2px 2px; }
.report-stats { margin-top: 22px; }
.report-department-list { max-width: 650px; }

.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: rgba(25, 32, 49, 0.48);
  backdrop-filter: blur(3px);
}
.confirm-modal {
  width: min(100%, 420px);
  padding: 28px;
  border: 1px solid #edf0f5;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 24px 70px rgba(26, 34, 54, 0.22);
  text-align: center;
  animation: modal-appear .16s ease-out;
}
.modal-warning-icon {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  margin: 0 auto 15px;
  border-radius: 50%;
  background: #fff0f1;
  color: #d14350;
  font-size: 25px;
  font-weight: 700;
}
.confirm-modal h2 {
  margin: 0 0 10px;
  color: #263247;
  font-size: 20px;
}
.confirm-modal p {
  margin: 0;
  color: #7c879a;
  font-size: 13px;
  line-height: 1.7;
  overflow-wrap: anywhere;
}
.confirm-modal p strong { color: #344057; }
.modal-actions {
  display: flex;
  justify-content: center;
  gap: 10px;
  flex-wrap: wrap;
  margin-top: 24px;
}
.modal-actions button { min-height: 40px; }
.modal-delete-button {
  border: 0;
  border-radius: 8px;
  padding: 10px 14px;
  background: #d14350;
  color: #fff;
  font-size: 11px;
  font-weight: 600;
}
.modal-delete-button:hover { background: #b93240; }
.modal-delete-button:disabled { opacity: .55; cursor: not-allowed; }
@keyframes modal-appear {
  from { opacity: 0; transform: translateY(6px) scale(.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

@media (max-width: 1150px) {
  .main-area { padding: 26px 22px 18px; }
  .dashboard { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .content-grid { grid-template-columns: minmax(0, 1fr); }
}

@media (max-width: 760px) {
  .sidebar {
    width: 68px;
    padding: 18px 8px;
    align-items: center;
  }
  .brand { padding: 0 0 28px; }
  .brand > div:last-child, .sidebar-label, .sidebar-bottom { display: none; }
  .brand-mark { width: 38px; height: 38px; flex: 0 0 38px; }
  .side-nav { width: 100%; }
  .nav-item { justify-content: center; padding: 13px 0; font-size: 0; }
  .nav-icon { font-size: 19px; }
  .main-area {
    margin-left: 68px;
    width: calc(100% - 68px);
    padding: 20px 13px;
  }
  .topbar { align-items: flex-start; flex-wrap: wrap; }
  h1 { font-size: 22px; }
  .today-label { display: none; }
  .topbar-actions { gap: 0; }
  .primary-button { padding: 10px 11px; }
  .employee-form { grid-template-columns: 1fr; }
  .form-actions { grid-column: auto; }
  .filters { flex-direction: column; }
  .filters select { width: 100%; }
  .footer { flex-direction: column; }
}

@media (max-width: 520px) {
  .dashboard { grid-template-columns: minmax(0, 1fr); }
  .stat-card { padding: 15px; }
  .panel { padding: 15px; }
  .topbar { flex-direction: column; }
  .topbar-actions { width: 100%; }
  .pagination { align-items: flex-start; }
  .salary-value { font-size: 20px; }
}
</style>
