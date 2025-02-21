import "../App.css";
import React from "react";

function RollPanel () {

  return (
      <div class="card flex-grow-5">

        <div class="card-header">
          Rolls
        </div>

        <div class="card-body p-10">

          <div>

          <div class="button-row m-10">

            <div class="w-100 form-group flex-grow-5 ">
              <label>🎲</label>
              <input type="text" placeholder="1d100"></input> 
          
    
            </div>
            <div class="form-group">
              <label>Norm</label>
              <input type="radio" name="adv-dis" checked className="ml-20px"/>
            </div>
            <div class="form-group">
              <label>Disadv</label>
              <input type="radio" name="adv-dis" className="ml-20px"/>
            </div>
            <div class="form-group">
              <label>Adv</label>              
              <input type="radio" name="adv-dis" className="ml-20px"/>
            </div>
          </div>

          <div class="flex-row w-100 m-10 flex-wrap">

            <div class="form-group">
              <input type="checkbox"></input> 
              <label>+Str</label>
            </div>

            <div class="form-group">
              <input type="checkbox"></input> 
              <label>+Con</label>
            </div>
            <div class="form-group">
              <input type="checkbox"></input> 
              <label>+Agl</label>
            </div>
            <div class="form-group">
              <input type="checkbox"></input> 
              <label>+Wis</label>
            </div>
            <div class="form-group">
              <input type="checkbox"></input> 
              <label>+Cha</label>
            </div>

            
          </div>          
          <div class="m-10">
              <input class="w-100"/>
           </div>            
          <div class="m-10">
            <button class="w-100 btn btn-primary">Roll</button>                 
           </div>               



          
          <div class="scrollable m-10">
            <table class="roll-list w-100">
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
      </div>
    )
  
}

export default RollPanel
