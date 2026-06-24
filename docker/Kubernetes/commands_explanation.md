# Explanation of Kubernetes and Docker Commands

This document provides a detailed explanation of the commands used in the terminal session to interact with Docker, Kubernetes, and the PostgreSQL database container.

---

## 1. Docker Information

### `docker version`
* **Description**: Displays details about the installed Docker client and server components (such as Version, API version, OS/Arch, Containerd, runc, etc.).
* **Use Case**: Helpful for verifying that the Docker daemon is running and check compatibility of the Docker client with the engine.

---

## 2. Kubernetes Configuration & Cluster Info

### `kubectl version --client`
* **Description**: Displays the client-side version of the Kubernetes command-line utility (`kubectl`).
* **Use Case**: Used to verify `kubectl` is installed and check its version.

### `kubectl cluster-info`
* **Description**: Prints endpoint information about the control plane and core services running in the cluster.
* **Use Case**: Useful for checking if the Kubernetes cluster is healthy and accessible.
* *Note on Error*: If Docker Desktop is stopped or Kubernetes is starting up, this command will fail with a connection refused error (`dial tcp [::1]:8080: connect: connection refused`).

### `kubectl config get-contexts`
* **Description**: Lists all the cluster contexts defined in your kubeconfig file (usually located at `~/.kube/config`). The context marked with an asterisk (`*`) is the current active context.
* **Use Case**: Useful when managing multiple clusters to verify which cluster and namespace you are currently pointing to.

### `kubectl get nodes`
* **Description**: Lists all the physical or virtual machines (nodes) that make up the Kubernetes cluster.
* **Use Case**: Verify that the cluster control-plane and worker nodes are in a `Ready` status.

---

## 3. Inspecting Kubernetes Resources

### `kubectl get pods`
* **Description**: Lists all the running Pods in the current namespace.
* **Use Case**: Checking if applications are successfully running (`STATUS: Running`) or debugging start failures.

### `kubectl get pods --watch`
* **Description**: Lists all running Pods in the current namespace and keeps the terminal session open to display real-time updates (such as state changes or container creation events).
* **Use Case**: Monitoring the dynamic changes of pods during scaling, rollout updates, or self-healing events in real-time.

### `kubectl get deployments`
* **Description**: Lists all Deployments in the current namespace.
* **Use Case**: Verifying how many replicas of an application are configured, and how many are currently up-to-date and available.

### `kubectl get services`
* **Description**: Lists all Services (networking interfaces) in the current namespace.
* **Use Case**: Finding the internal ClusterIP, External IP, and mapped port definitions of your applications.

---

## 4. Applying Configurations

### `kubectl apply -f <filename>`
* **Description**: Creates or updates Kubernetes resources defined in a local YAML configuration file.
* **Use Case**: Running `kubectl apply -f 01-postgres.yaml` creates the PostgreSQL Deployment and Service resources in the cluster as configured.
* *Note on Error*: Running this command outside the folder containing the file returns `error: the path "01-postgres.yaml" does not exist`. You must first navigate (`cd`) to the correct folder.

---

## 5. Executing Commands & Interacting with Containers

### Running `kubectl exec`
The `kubectl exec` command is used to run a process/command inside an existing container.

#### ❌ Failed Attempt 1: `kubectl exec -it <pod-name>`
* **Error**: `error: you must specify at least one command for the container`
* **Explanation**: You cannot just open a terminal without specifying the command shell (e.g., `bash`, `sh`) or the utility (e.g., `psql`) to run.

#### ❌ Failed Attempt 2: `kubectl exec -it <pod-name> --psql -U appuser -d appdb`
* **Error**: `error: unknown flag: --psql`
* **Explanation**: Without a double-dash (`--`), `kubectl` tries to parse `--psql` as a flag for the `kubectl exec` command itself rather than passing it to the container's shell.

####  Successful Syntax: `kubectl exec -it <pod-name> -- psql -U appuser -d appdb`
* **Explanation**: The `--` tells `kubectl` to stop parsing arguments, and everything after `--` is passed directly as the command running inside the container.
* **Parameters**:
  * `-i` (interactive): Keeps `stdin` open.
  * `-t` (tty): Allocates a pseudo-TTY (gives you an interactive terminal prompt).
  * `psql`: The PostgreSQL command-line tool.
  * `-U appuser`: Connects as the user `appuser`.
  * `-d appdb`: Connects to the database `appdb`.

---

## 6. PostgreSQL CLI (`psql`) Commands

Once inside the PostgreSQL interactive terminal (`appdb=#`), the following commands are used:

* **`\dt`**
  * **Description**: Lists all tables (relations) in the current database.
  * **Output**: `Did not find any relations.` (means no tables have been created in the `appdb` database yet).
* **`\q`**
  * **Description**: Exits/quits the PostgreSQL interactive terminal.

---

## 7. Resource Description & Details

### `kubectl describe service <service-name>` (or `svc`)
* **Description**: Displays detailed configurations, metadata, active selectors, and internal events of a specific Service.
* **Use Case**: Crucial for checking the **Endpoints** field to verify if the Service is successfully mapping to the IP addresses of running pods.
* **Terminal Example**: Running `kubectl describe svc nginx-service` showed target port mappings and listed three distinct endpoints corresponding to the three running replica pods: `Endpoints: 10.244.0.9:80,10.244.0.8:80,10.244.0.7:80`.

### `kubectl describe deployment <deployment-name>`
* **Description**: Shows exhaustive information about a Deployment (desired replicas, active strategy, rolling update settings, conditions, and scaling event history).
* **Use Case**: Checking the current status of replicas, troubleshooting update stuck errors, or checking controller operations.
* **Terminal Example**: Running `kubectl describe deployment nginx-deployment` verified the `RollingUpdateStrategy` (25% max unavailable, 25% max surge) and listed scaling event logs showing the `deployment-controller` scaling replicas from 3 to 5.

---

## 8. Scaling Deployments

### `kubectl scale deployment <deployment-name> --replicas=<count>`
* **Description**: Manually scales a Deployment's replica count up or down.
* **Use Case**: Instantly adjusting application capacity to handle shifts in load.
* **Terminal Example**: `kubectl scale deployment nginx-deployment --replicas=5` scaled up the running pods of the Nginx deployment from 3 to 5.

---

## 9. Kubernetes Self-Healing & Pod Lifecycle

### `kubectl delete pod <pod-name>`
* **Description**: Deletes/terminates a running Pod.
* **Use Case**: Testing container resilience or simulating node failures.

### 💡 Understanding Self-Healing
Kubernetes is **declarative**. You define the **desired state** (e.g., "I want 3 replicas of Nginx"), and the Kubernetes controller loop constantly reconciles the **actual state** with the desired state. When a mismatch is detected, Kubernetes self-heals by executing necessary actions.

#### Demonstrated Self-Healing Events in Terminal:
1. **PostgreSQL Pod Self-Healing**:
   * **Action**: Executed `kubectl delete pod postgres-7fbcbb5d5d-rxvgh`.
   * **Self-Healing Response**: Because the pod was managed by a Deployment (desired replicas: 1), Kubernetes immediately detected the pod deletion (actual: 0) and automatically spawned a new pod (`postgres-7fbcbb5d5d-rt4fh`) to restore the desired count.
2. **Nginx Pod Self-Healing**:
   * **Action**: Executed `kubectl delete pod nginx-deployment-7ccccd94f7-5dx5h`.
   * **Self-Healing Response**: The Deployment (desired replicas: 3) detected the missing replica (actual: 2) and instantly launched a new pod (`nginx-deployment-7ccccd94f7-7nnc9`) to restore the service back to full capacity.

