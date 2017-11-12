import { fetch, addTask } from "domain-task";

export const createToggle = (name, description, success, failure) => (
  dispatch,
  getState
) => {
  const options = {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      name: name,
      description: description
    })
  };

  const fetchTask = fetch(`/toggles`, options).then(response => {
    if (response.status === 201) {
      dispatch({ type: "CREATE_TOGGLE_SUCCESS" });
      success();
      return;
    }

    response.json().then(body => {
      dispatch({ type: "CREATE_TOGGLE_FAILURE", messages: body.messages });
      failure(body);
    });
  });

  addTask(fetchTask);
  dispatch({ type: "CREATE_TOGGLE_REQUEST" });
};
