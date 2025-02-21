import logo from "./logo.svg";
import "./App.css";

function Globe(value, max) {
  return (
    <div class="card w-100 m-10">
      <div class="card-header">Hunger</div>

      <div class="card-body">
        <progress
          class="orb orb-yellow m-bottom-10"
          value="8"
          max="10"
        ></progress>

        <div class="flex-row m-auto">
          <div class="m-auto">8</div>
          <div class="m-auto">|</div>

          <div class="m-auto">10</div>
        </div>

        <div class="m-bottom-10">♻️ 15</div>
      </div>
    </div>
  );
}

function App() {
  return (
    <div className="App">
      <Globe />
    </div>
  );
}

export default App;
