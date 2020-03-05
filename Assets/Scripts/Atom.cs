using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom
{
    public int serial_;
    public string name_;
    public char alt_loc_;
    public string res_name_;
    public char chain_id_;
    public int res_seq_;
    public char i_code_;
    
    /* Stored in nanometers */
    public float x_;
    public float y_;
    public float z_;

    public float occupancy_;
    public float temp_factor_;
    public string element_;
    public string charge_;

    public Atom() {

    }

    public Atom(int serial, string name, char alt_loc, string res_name, char chain_id,
        int res_seq, char i_code, float x, float y, float z, float occupancy,
        float temp_factor, string element, string charge) 
    {
        serial_ = serial;
        name_ = name;
        alt_loc_ = alt_loc;
        res_name_ = res_name;
        chain_id_ = chain_id;
        res_seq_ = res_seq;
        i_code_ = i_code;
        x_ = x;
        y_ = y;
        z_ = z;
        occupancy_ = occupancy;
        temp_factor_ = temp_factor;
        element_ = element;
        charge_ = charge;
    }

}
