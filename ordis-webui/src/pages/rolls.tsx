import "../App.css";
import React from "react";

function RollPanel() {

  return (
    <div>
      <div className="card flex-grow-5">

        <div className="card-header">
          Rolls
        </div>

        <div className="card-body p-10">

          <div>

            <div className="button-row m-10">

              <div className="w-100 form-group flex-grow-5 ">
                <label>🎲</label>
                <input type="text" placeholder="1d100"></input>


              </div>
              <div className="form-group">
                <label>Norm</label>
                <input type="radio" name="adv-dis" checked classNameName="ml-20px" />
              </div>
              <div className="form-group">
                <label>Disadv</label>
                <input type="radio" name="adv-dis" classNameName="ml-20px" />
              </div>
              <div className="form-group">
                <label>Adv</label>
                <input type="radio" name="adv-dis" classNameName="ml-20px" />
              </div>
            </div>

            <div className="flex-row w-100 m-10 flex-wrap">

              <div className="form-group">
                <input type="checkbox"></input>
                <label>+Str</label>
              </div>

              <div className="form-group">
                <input type="checkbox"></input>
                <label>+Con</label>
              </div>
              <div className="form-group">
                <input type="checkbox"></input>
                <label>+Agl</label>
              </div>
              <div className="form-group">
                <input type="checkbox"></input>
                <label>+Wis</label>
              </div>
              <div className="form-group">
                <input type="checkbox"></input>
                <label>+Cha</label>
              </div>


            </div>
            <div className="m-10">
              <input className="w-100" />
            </div>
            <div className="m-10">
              <button className="w-100 btn btn-primary">Roll</button>
            </div>
          </div>

        </div>
        <br />

      </div>
      <div>

      <div className="scrollable m-10 d-none">
        <table className="roll-list w-100">
          <tr>
            <th>Time</th>
            <th>Dice</th>
            <th>Rolls</th>
            <th>Result</th>
          </tr>
          <tr>
            <td>5 minutes ago</td>
            <td>1d100+str</td>
            <td>24,59,14</td>
            <td>48</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>

            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>12</td>
          </tr>
          <tr>
            <td>10 minutes ago</td>
            <td>1d100+agl</td>
            <td>24,59,14,24,59,14,24,59,14,24,59,14,24,59,14,24,59,14</td>
            <td>23</td>
          </tr>
        </table>
      </div>        
      </div>

    </div>
  )

}

export default RollPanel
